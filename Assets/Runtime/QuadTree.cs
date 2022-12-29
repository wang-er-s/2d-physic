using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class QuadTree
{

    private static class QuadTreePool
    {

        ///// Fields /////

        private static Queue<QuadTree> _pool;
        private static int _maxPoolCount = 1024;
        private static int _defaultMaxBodiesPerNode = 6;
        private static int _defaultMaxLevel = 6;

        ///// Methods /////

        public static QuadTree GetQuadTree(Rect bounds, QuadTree parent)
        {
            if (_pool == null) Init();
            QuadTree tree = null;
            if (_pool.Count > 0)
            {
                tree = _pool.Dequeue();
                tree._bounds = bounds;
                tree._parent = parent;
                tree._maxLevel = parent._maxLevel;
                tree._maxBodiesPerNode = parent._maxBodiesPerNode;
                tree._curLevel = parent._curLevel + 1;
                tree.selfAABB = new AABB(bounds.min, bounds.max);
                tree.isRelease = false;
            }
            else tree = new QuadTree(bounds, parent);

            return tree;
        }

        public static void PoolQuadTree(QuadTree tree)
        {
            if (tree == null) return;
            tree.Clear();
            if (_pool.Count > _maxPoolCount) return;
            _pool.Enqueue(tree);
        }

        private static void Init()
        {
            _pool = new Queue<QuadTree>();
            for (int i = 0; i < _maxPoolCount; i++)
            {
                _pool.Enqueue(new QuadTree(Rect.zero, _defaultMaxBodiesPerNode, _defaultMaxLevel));
            }
        }
    }

    ///// Constructors /////
    private static int count;
    private int id;
    public QuadTree(Rect bounds, int maxBodiesPerNode = 6, int maxLevel = 6)
    {
        id = count++;
        _bounds = bounds;
        _maxBodiesPerNode = maxBodiesPerNode;
        _maxLevel = maxLevel;
        _bodies = new List<MRigidbody>(maxBodiesPerNode);
        selfAABB = new AABB(bounds.min, bounds.max);
    }

    private QuadTree(Rect bounds, QuadTree parent)
        : this(bounds, parent._maxBodiesPerNode, parent._maxLevel)
    {
        _parent = parent;
        _curLevel = parent._curLevel + 1;
    }

    ///// Fields /////

    private QuadTree _parent;
    private Rect _bounds;
    private List<MRigidbody> _bodies;
    private int _maxBodiesPerNode;
    private int _maxLevel;
    private int _curLevel;
    private bool isRelease;
    private QuadTree _childA;
    private QuadTree _childB;
    private QuadTree _childC;
    private QuadTree _childD;

    private List<MRigidbody> _entCache;
    private static Dictionary<MRigidbody, List<QuadTree>> body2QuadTree = new();
    private AABB selfAABB;

    private void AddBodyToList(MRigidbody body)
    {
        if (!_bodies.Contains(body))
            _bodies.Add(body);
        if (!body2QuadTree.TryGetValue(body, out var list))
        {
            list = new List<QuadTree>();
            body2QuadTree[body] = list;
        }

        if (!list.Contains(this))
            list.Add(this);
    }

    private void AddBodyToList(IEnumerable<MRigidbody> bodies)
    {
        foreach (var body in bodies)
        {
            AddBodyToList(body); 
        }
    }

    private void RemoveBodyFromList(MRigidbody body)
    {
        RemoveBodyFromList(_bodies.IndexOf(body)); 
    }
    
    private void RemoveBodyFromList(int index)
    {
        var body = _bodies[index];
        _bodies.RemoveAt(index);
        if (body2QuadTree.TryGetValue(body, out var list))
        {
            list.Remove(this);
            if (list.Count <= 0)
            {
                body2QuadTree.Remove(body);
            }
        }
        else
        {
            throw new Exception("删除了没有添加的刚体");
        }
    }
    

    public void AddBody(MRigidbody body)
    {
        if (_childA != null)
        {
            using var children = IntersectMultiChild(body.GetAABB());
            if (children.Count > 1)
            {
                AddBodyToList(body);
            }
            else
            {
                children[0].AddBody(body);
            }
        }
        else
        {
            AddBodyToList(body);
            if (_bodies.Count > _maxBodiesPerNode && _curLevel < _maxLevel)
            {
                Split();
            }
        }
    }

    public void UpdateBody(MRigidbody body)
    {
        if (!body2QuadTree.TryGetValue(body, out var quadTrees))
        {
            return;
        }

        AABB aabb = body.GetAABB();
        using var useQuad = RecyclableList<QuadTree>.Create();
        useQuad.AddRange(quadTrees);
        foreach (var quadTree in useQuad)
        {
            if (quadTree.AABBQuadIntersect(aabb))
            {
                continue;
            }
            quadTree.RemoveBodyInternal(body);
        }
        
        AddBody(body);
    }

    public void RemoveBody(MRigidbody body)
    {
        if (!body2QuadTree.TryGetValue(body, out var quadTrees))
        {
            return;
        }

        using var useQuad = RecyclableList<QuadTree>.Create();
        useQuad.AddRange(quadTrees);
        foreach (var quadTree in useQuad)
        {
            quadTree.RemoveBodyInternal(body);
        }
    }

    private int GetAllBodyCount()
    {
        if (_childA == null)
            return _bodies.Count;
        return _childA.GetAllBodyCount() + _childB.GetAllBodyCount() + _childC.GetAllBodyCount() +
               _childD.GetAllBodyCount() + _bodies.Count;
    }

    private void RemoveBodyInternal(MRigidbody body)
    {
        if(isRelease) return;
        RemoveBodyFromList(body);
        var parent = _parent;
        if (parent == null) return;
        if (parent.GetAllBodyCount() <= _maxBodiesPerNode)
        {
            parent.AddBodyToList(parent._childA._bodies);
            parent.AddBodyToList(parent._childB._bodies);
            parent.AddBodyToList(parent._childC._bodies);
            parent.AddBodyToList(parent._childD._bodies);

            QuadTreePool.PoolQuadTree(parent._childA);
            QuadTreePool.PoolQuadTree(parent._childB);
            QuadTreePool.PoolQuadTree(parent._childC);
            QuadTreePool.PoolQuadTree(parent._childD);

            parent._childA = null;
            parent._childB = null;
            parent._childC = null;
            parent._childD = null;
        }
    }

    public IReadOnlyList<MRigidbody> GetBodies(AABB aabb)
    {
        if (_entCache == null) _entCache = new List<MRigidbody>(64);
        else _entCache.Clear();
        GetBodies(aabb, _entCache);
        return _entCache;
    }

    public IReadOnlyList<MRigidbody> GetBodies(MRigidbody rigidbody)
    {
        return GetBodies(rigidbody.GetAABB());
    }

    private void GetBodies(AABB aabb, List<MRigidbody> bods)
    {
        //no children
        bods.AddRange(_bodies);
        if (_childA != null)
        {
            if (_childA.ContainsAABB(aabb))
                _childA.GetBodies(aabb, bods);
            if (_childB.ContainsAABB(aabb))
                _childB.GetBodies(aabb, bods);
            if (_childC.ContainsAABB(aabb))
                _childC.GetBodies(aabb, bods);
            if (_childD.ContainsAABB(aabb))
                _childD.GetBodies(aabb, bods);
        }
    }

    public bool ContainsAABB(AABB aabb)
    {
        return PhysicsRaycast.AABBIntersect(aabb, this.selfAABB);
    }

    private void Split()
    {
        var hx = _bounds.width / 2;
        var hz = _bounds.height / 2;
        var sz = new Vector2(hx, hz);

        //split a
        var aLoc = _bounds.position;
        var aRect = new Rect(aLoc, sz);
        //split b
        var bLoc = new Vector2(_bounds.position.x + hx, _bounds.position.y);
        var bRect = new Rect(bLoc, sz);
        //split c
        var cLoc = new Vector2(_bounds.position.x + hx, _bounds.position.y + hz);
        var cRect = new Rect(cLoc, sz);
        //split d
        var dLoc = new Vector2(_bounds.position.x, _bounds.position.y + hz);
        var dRect = new Rect(dLoc, sz);

        //assign QuadTrees
        _childA = QuadTreePool.GetQuadTree(aRect, this);
        _childB = QuadTreePool.GetQuadTree(bRect, this);
        _childC = QuadTreePool.GetQuadTree(cRect, this);
        _childD = QuadTreePool.GetQuadTree(dRect, this);

        for (int i = 0; i < _bodies.Count; i++)
        {
            var quads = IntersectMultiChild(_bodies[i].GetAABB());
            if (quads.Count <= 1)
            {
                quads[0].AddBody(_bodies[i]);
                RemoveBodyFromList(i);
                i--;
            }
        }
    }

    private QuadTree GetLowestQuad(AABB aabb)
    {
        List<QuadTree> searchQuad = new() { this };
        List<QuadTree> newChildren = new();
        while (true)
        {
            newChildren.Clear();
            for (int i = 0; i < searchQuad.Count; i++)
            {
                var quad = searchQuad[i].GetQuadrant(aabb);
                if (quad == searchQuad[i])
                {
                    return quad;
                }
                else
                {
                    newChildren.Add(quad);
                }
            }
            if (newChildren.Count > 0)
            {
                searchQuad.Clear();
                searchQuad.AddRange(newChildren);
            }
        }
    }

    private QuadTree GetQuadrant(AABB aabb)
    {
        if (_childA == null) return this;
        if (_childA.AABBQuadIntersect(aabb))
        {
            return _childA;
        }
        if (_childB.AABBQuadIntersect(aabb))
        {
            return _childB;
        }
        if (_childC.AABBQuadIntersect(aabb))
        {
            return _childC;
        }
        if (_childD.AABBQuadIntersect(aabb))
        {
            return _childD;
        }

        throw new Exception("超出范围");
    }
    
    private RecyclableList<QuadTree> IntersectMultiChild(AABB aabb)
    {
        RecyclableList<QuadTree> result = RecyclableList<QuadTree>.Create();
        if (_childA == null) return result;
        if (_childA.AABBQuadIntersect(aabb))
        {
            result.Add(_childA);
        }
        if (_childB.AABBQuadIntersect(aabb))
        {
            result.Add(_childB);
        }
        if (_childC.AABBQuadIntersect(aabb))
        {
            result.Add(_childC);
        }
        if (_childD.AABBQuadIntersect(aabb))
        {
            result.Add(_childD);
        }

        return result;
    }

    private bool AABBQuadIntersect(AABB aabb)
    {
        return PhysicsRaycast.AABBRectIntersect(aabb, _bounds);
    }

    private void Clear()
    {
        QuadTreePool.PoolQuadTree(_childA);
        QuadTreePool.PoolQuadTree(_childB);
        QuadTreePool.PoolQuadTree(_childC);
        QuadTreePool.PoolQuadTree(_childD);
        _childA = null;
        _childB = null;
        _childC = null;
        _childD = null;
        while (_bodies.Count > 0)
        {
            RemoveBodyFromList(0);
        }
        _bodies.Clear();
        isRelease = true;
    }

    public void DrawGizmos()
    {
        //draw children
        if (_childA != null) _childA.DrawGizmos();
        if (_childB != null) _childB.DrawGizmos();
        if (_childC != null) _childC.DrawGizmos();
        if (_childD != null) _childD.DrawGizmos();

        //draw rect
        Gizmos.color = Color.cyan;
        var p1 = new Vector3(_bounds.position.x, 0.1f, _bounds.position.y);
        var p2 = new Vector3(p1.x + _bounds.width, 0.1f, p1.z);
        var p3 = new Vector3(p1.x + _bounds.width, 0.1f, p1.z + _bounds.height);
        var p4 = new Vector3(p1.x, 0.1f, p1.z + _bounds.height);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
        StringBuilder sb = new StringBuilder();
        foreach (var body in _bodies)
        {
            sb.Append($"{body.Id} ");
        }
        Handles.color = Color.red;
        Handles.Label(p1, sb.ToString());
        return; 
        Handles.color = Color.yellow; 
        Handles.Label(new Vector3(_bounds.x + _bounds.width / 2,0.1f, _bounds.y + _bounds.height / 2), id.ToString());
    }
}