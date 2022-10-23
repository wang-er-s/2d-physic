﻿using System;

namespace _3DDataType
{
    public struct MVector2 : IEquatable<MVector2>
    {
        private static readonly MVector2 zeroVector2 = new MVector2(0.0f, 0.0f);
        private static readonly MVector2 oneVector2 = new MVector2(1, 1);

        public static MVector2 Zero => zeroVector2;
        public static MVector2 One => oneVector2;


        public float x;
        public float y;

        public float magnitude => (float) Math.Sqrt(x * x + y * y);
        public float sqrMagnitude => x * x + y * y;

        public static float Dot(MVector2 v1, MVector2 v2)
        {
            return (float) ((double) v1.x * (double) v2.x + (double) v1.y * (double) v2.y);
        }

        public MVector2(float x = 0.0f, float y = 0.0f)
        {
            this.x = x;
            this.y = y;
        }

        public static MVector2 operator +(MVector2 v1, MVector2 v2)
        {
            return new MVector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static MVector2 operator -(MVector2 v1, MVector2 v2)
        {
            return new MVector2(v1.x - v2.x, v1.y - v2.y);
        }

        public static MVector2 operator *(MVector2 v1, MVector2 v2)
        {
            return new MVector2(v1.x * v2.x, v1.y * v2.y);
        }

        public static MVector2 operator /(MVector2 v1, MVector2 v2)
        {
            return new MVector2(v1.x / v2.x, v1.y / v2.y);
        }

        public static MVector2 operator *(MVector2 v1, float num)
        {
            return new MVector2(v1.x * num, v1.y * num);
        }

        public static MVector2 operator /(MVector2 v1, float num)
        {
            return new MVector2(v1.x / num, v1.y / num);
        }

        public static MVector2 operator -(MVector2 v1)
        {
            return new MVector2(-v1.x, -v1.y);
        }

        public static bool operator ==(MVector2 v1, MVector2 v2)
        {
            return (v1 - v2).magnitude < 1e-7;
        }

        public static bool operator !=(MVector2 v1, MVector2 v2)
        {
            return !(v1 == v2);
        }

        public bool Equals(MVector2 other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MVector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                return hashCode;
            }
        }

        public void Normalize()
        {
            if (magnitude > 0)
                this /= this / magnitude;
            else
                this = Zero;
        }
    }
}