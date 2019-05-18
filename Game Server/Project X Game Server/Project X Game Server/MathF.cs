using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public class MathF
    {
        private static long LastTick;
        public static float deltaTime
        {
            get
            {
                long CurrentTick = Environment.TickCount;
                long Result = CurrentTick - LastTick;
                LastTick = CurrentTick;
                return Result / 1000.0f;
            }
        }
        public static float Distance(Entity e1, Entity e2)
        {
            if (e1 == null || e2 == null)
            {
                return 999.0f;
            }
            return Vector3.Distance(e1.position, e2.position);
        }
        public static float Distance(Entity e1, Vector3 e2)
        {
            if (e1 == null)
            {
                return 999.0f;
            }
            return Vector3.Distance(e1.position, e2);
        }
        public static void MoveTowards(Entity e, Entity target, float Step)
        {
            if (e == null || target == null)
            {
                return;
            }
            e.position = e.position + (target.position - e.position).normalized * Step;
        }
        public static void MoveTowards(Entity e, Vector3 target, float Step)
        {
            if (e == null)
            {
                return;
            }
            e.position = e.position + (target - e.position).normalized * Step;
        }
        public static int Damage(int Strength, int Agility, int BloodMultiplier, out bool Crit)
        {
            Random random = new Random();

            int RandDamage = random.Next((int)(Strength * 0.9f), (int)(Strength * 1.1f));
            Crit = (random.Next(0, 100) >= 90 - Agility ? true : false);
            return (Crit ? (RandDamage * 2 * BloodMultiplier) : (RandDamage * BloodMultiplier));
        }
        public static int SpellDamage(int MinDamage, int MaxDamage, int CritChance, out bool Crit)
        {
            Random random = new Random();
            int RandDamage = random.Next(MinDamage, MaxDamage);
            Crit = (random.Next(0, 100) >= 100 - CritChance ? true : false);
            return (Crit ? (RandDamage * 2) : (RandDamage));
        }
        public static void LookAt(Entity baseEntity, Entity lookAtEntity)
        {
            if (baseEntity == null || lookAtEntity == null)
            {
                return;
            }
            float xDiff = lookAtEntity.position.x - baseEntity.position.x;
            float zDiff = lookAtEntity.position.z - baseEntity.position.z;
            baseEntity.r = (float)(Math.Atan2(xDiff, zDiff) * (180 / Math.PI));
        }
        public static float RadiansToDegrees(float r)
        {
            return (float)(180.0f / Math.PI) * r;
        }
    }
    public class DamageResponse
    {
        public int NPC_Entity_ID = -1;
        public int Damage = 0;
        public bool Crit = false;
        public int New_HP = -1;
        public DamageResponse(int npc_Entity_ID, int damage, bool crit, int new_HP)
        {
            NPC_Entity_ID = npc_Entity_ID;
            Damage = damage;
            Crit = crit;
            New_HP = new_HP;
        }
    }
    public class Vector3
    {
        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;
        public Vector3 normalized
        {
            get
            {
                float len = (float)Math.Sqrt((x * x) + (y * y) + (z * z));
                float nx = (x == 0 || len == 0) ? 0 : x / len;
                float ny = (y == 0 || len == 0) ? 0 : y / len;
                float nz = (z == 0 || len == 0) ? 0 : z / len;
                return new Vector3(nx, ny, nz);
            }
        }
        public static Vector3 up
        {
            get
            {
                return new Vector3(0, 1, 0);
            }
        }
        public static Vector3 forward
        {
            get
            {
                return new Vector3(0, 0, 1);
            }
        }
        public Vector3(float _x = 0.0f, float _y = 0.0f, float _z = 0.0f)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        // General functions
        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }
        public float Distance(Vector3 v1)
        {
            return (float)Math.Sqrt((x - v1.x) * (x - v1.x) + (z - v1.z) * (z - v1.z));
        }
        public static float Distance(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Sqrt((v2.x - v1.x) * (v2.x - v1.x) + (v2.z - v1.z) * (v2.z - v1.z));
        }

        // Operator Overrides
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return (v1.x == v2.x && v1.y == v2.y && v1.z == v2.z);
        }
        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return (v1.x != v2.x || v1.y != v2.y || v1.z != v2.z);
        }
        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
        public static Vector3 operator *(Vector3 v1, float f)
        {
            return new Vector3(v1.x * f, v1.y * f, v1.z * f);
        }
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
    }
    public class Quaternion
    {
        public float x = 0.0f;
        private float sx
        {
            get
            {
                return x * x;
            }
        }
        public float y = 0.0f;
        private float sy
        {
            get
            {
                return y * y;
            }
        }
        public float z = 0.0f;
        private float sz
        {
            get
            {
                return z * z;
            }
        }
        public float w = 0.0f;
        private float sw
        {
            get
            {
                return w * w;
            }
        }
        public static Quaternion identity
        {
            get
            {
                return new Quaternion(0, 0, 0, 1);
            }
        }
        public Vector3 ToEulerAngles
        {
            get
            {
                return new Vector3
                    (
                        (float)Math.Atan2(2.0f * x * w + 2.0f * y * z, 1.0f - 2.0f * (sz + sw)),
                        (float)Math.Asin(2.0f * (x * z - w * y)),
                        (float)Math.Atan2(2.0f * x * y + 2.0f * z * w, 1.0f - 2.0f * (sy + sz))
                    );
            }
        }
        public Quaternion(float _x = 0.0f, float _y = 0.0f, float _z = 0.0f, float _w = 0.0f)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public static Quaternion LookAt(Vector3 forward)
        {
            float dot = Vector3.Dot(Vector3.forward, forward);
            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                return new Quaternion(Vector3.up.x, Vector3.up.y, Vector3.up.z, 3.1415926535897932f);
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                return identity;
            }
            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(Vector3.forward, forward);
            rotAxis = rotAxis.normalized;
            return CreateFromAxisAngle(rotAxis, rotAngle);
        }
        private static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float halfAngle = angle * 0.5f;
            float s = (float)Math.Sin(halfAngle);
            return new Quaternion(axis.x * s, axis.y * s, axis.z * s, (float)Math.Cos(halfAngle));
        }
    }
}
