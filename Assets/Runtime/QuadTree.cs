using System;
using System.Collections.Generic;
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

    public QuadTree(Rect bounds, int maxBodiesPerNode = 6, int maxLevel = 6)
    {
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
    private QuadTree _childA;
    private QuadTree _childB;
    private QuadTree _childC;
    private QuadTree _childD;
    private List<MRigidbody> _entCache;
    private AABB selfAABB;

    ///// Methods /////

    public void AddBody(MRigidbody body)
    {
        if (_childA != null)
        {
            var child = GetQuadrant(body.Position);
            child.AddBody(body);
        }
        else
        {
            _bodies.Add(body);
            if (_bodies.Count > _maxBodiesPerNode && _curLevel < _maxLevel)
            {
                Split();
            }
        }
    }

    public void UpdateBody(MRigidbody body)
    {
        RemoveBody(body);
        AddBody(body);
    }

    public void RemoveBody(MRigidbody body)
    {
        var quad = GetLowestQuad(body.Position);
        quad._bodies.Remove(body);
        var parent = quad._parent;
        if(parent == null) return;
        var totalCount = 0;
        totalCount += parent._childA._bodies.Count;
        totalCount += parent._childB._bodies.Count;
        totalCount += parent._childC._bodies.Count;
        totalCount += parent._childD._bodies.Count;
        if (totalCount <= _maxBodiesPerNode)
        {
            parent._bodies.AddRange(parent._childA._bodies);
            parent._bodies.AddRange(parent._childB._bodies);
            parent._bodies.AddRange(parent._childC._bodies);
            parent._bodies.AddRange(parent._childD._bodies);
            
            QuadTreePool.PoolQuadTree(parent._childA);
            QuadTreePool.PoolQuadTree(parent._childB);
            QuadTreePool.PoolQuadTree(parent._childC);
            QuadTreePool.PoolQuadTree(parent._childD);
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
        if (_childA == null)
        {
            for (int i = 0; i < _bodies.Count; i++)
                bods.Add(_bodies[i]);
        }
        else
        {
            // todo 大方块跨越多个quad，导致判断错误
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

        for (int i = _bodies.Count - 1; i >= 0; i--)
        {
            var child = GetQuadrant(_bodies[i].Position);
            child.AddBody(_bodies[i]);
            _bodies.RemoveAt(i);
        }
    }

    private QuadTree GetLowestQuad(Vector2 point)
    {
        var ret = this;
        while (true)
        {
            var newChild = ret.GetQuadrant(point);
            if (newChild != null) ret = newChild;
            else break;
        }

        return ret;
    }
    
    private QuadTree GetQuadrant(Vector2 point)
    {
        if (_childA == null) return null;
        if (point.x > _bounds.x + _bounds.width / 2)
        {
            if (point.y > _bounds.y + _bounds.height / 2) return _childC;
            else return _childB;
        }
        else
        {
            if (point.y > _bounds.y + _bounds.height / 2) return _childD;
            return _childA;
        }
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
        _bodies.Clear();
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
    }
}