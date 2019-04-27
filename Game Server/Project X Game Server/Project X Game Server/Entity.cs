using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public enum EntityType
    {
        NONE,
        Player,
        NPC,
        Object
    }
    public enum Gender
    {
        Male,
        Female,
        NA
    }
    public class Entity
    {
        public EntityType type;
        public int Entity_ID = -1;
        private string _Name = "";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    Changed = true;
                    _Name = value;
                }
            }
        }
        private int _Level = 1;
        public int Level
        {
            get
            {
                return _Level;
            }
            set
            {
                if (_Level != value)
                {
                    Changed = true;
                    _Level = value;
                }
            }
        }
        public Gender gender;
        private float _x = 0.0f;
        public float x
        {
            get
            {
                return _x;
            }
            set
            {
                if (value != _x)
                {
                    Changed = true;
                    _x = value;
                }
            }
        }
        private float _y = 0.0f;
        public float y
        {
            get
            {
                return _y;
            }
            set
            {
                if (value != _y)
                {
                    Changed = true;
                    _y = value;
                }
            }
        }
        private float _z = 0.0f;
        public float z
        {
            get
            {
                return _z;
            }
            set
            {
                if (value != _z)
                {
                    Changed = true;
                    _z = value;
                }
            }
        }
        private float _r = 0.0f;
        public float r
        {
            get
            {
                return _r;
            }
            set
            {
                if (value != _r)
                {
                    Changed = true;
                    _r = value;
                }
            }
        }
        private float _vx = 0.0f;
        public float vx
        {
            get
            {
                return _vx;
            }
            set
            {
                if (value != _vx)
                {
                    Changed = true;
                    _vx = value;
                }
            }
        }
        private float _vy = 0.0f;
        public float vy
        {
            get
            {
                return _vy;
            }
            set
            {
                if (value != _vy)
                {
                    Changed = true;
                    _vy = value;
                }
            }
        }
        private float _vz = 0.0f;
        public float vz
        {
            get
            {
                return _vz;
            }
            set
            {
                if (value != _vz)
                {
                    Changed = true;
                    _vz = value;
                }
            }
        }
        private EntityType _TargetType = EntityType.NONE;
        public EntityType TargetType
        {
            get
            {
                return _TargetType;
            }
            set
            {
                if (_TargetType != value)
                {
                    Changed = true;
                    _TargetType = value;
                }
            }
        }
        private int _TargetID = -1;
        public int TargetID
        {
            get
            {
                return _TargetID;
            }
            set
            {
                if (_TargetID != value)
                {
                    Changed = true;
                    _TargetID = value;
                }
            }
        }
        private int max_HP = 100;
        public int Max_HP
        {
            get
            {
                return max_HP;
            }
        }
        private int current_HP = 100;
        public int Current_HP
        {
            get
            {
                return current_HP;
            }
            set
            {
                if (current_HP != value)
                {
                    Changed = true;
                    current_HP = value;
                }
            }
        }
        private int strength = 0;
        public int Strength
        {
            get
            {
                return strength;
            }
        }
        private int agility = 0;
        public int Agility
        {
            get
            {
                return agility;
            }
        }

        private bool inCombat = false;
        public bool InCombat
        {
            get
            {
                return inCombat;
            }
            set
            {
                if (inCombat != value)
                {
                    Changed = true;
                    inCombat = value;
                }
            }
        }

        public DateTime NextAttack = default(DateTime);

        public AnimationState AnimState = new AnimationState();

        protected bool Changed = false;

        protected ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

        public Entity(int ID, string name, int level, Gender _gender, float x, float y, float z, float r,
            float vX, float vY, float vZ, int HP, int Strength, int Agility)
        {
            Entity_ID = World.instance.EntityCounter;
            _Name = name;
            _Level = level;
            gender = _gender;
            _x = x;
            _y = y;
            _z = z;
            _r = r;
            _vx = vX;
            _vy = vY;
            _vz = vZ;
            max_HP = HP;
            current_HP = HP;
            strength = Strength;
            agility = Agility;
        }

        protected virtual void BuildTransmission(out byte[] data)
        {
            buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteInteger(Entity_ID);
            buffer.WriteFloat(_x);
            buffer.WriteFloat(_y);
            buffer.WriteFloat(_z);
            buffer.WriteFloat(_r);
            buffer.WriteFloat(_vx);
            buffer.WriteFloat(_vy);
            buffer.WriteFloat(_vz);
            buffer.WriteInteger(current_HP);
            bool b3 = false;
            bool b4 = false;
            bool b5 = false;
            bool b6 = false;
            bool b7 = false;
            byte bools = BitwiseRefinement.BoolsToByte
            (
                AnimState.Attacking,
                AnimState.Attacked,
                AnimState.Dead,
                b3,
                b4,
                b5,
                b6,
                b7
            );
            buffer.WriteByte(bools);
            switch (type)
            {
                case EntityType.Player:
                    buffer.WriteFloat(AnimState.Forward);
                    buffer.WriteFloat(AnimState.Turn);
                    buffer.WriteByte((byte)((AnimState.Crouch) ? 1 : 0));
                    buffer.WriteByte((byte)((AnimState.OnGround) ? 1 : 0));
                    buffer.WriteFloat(AnimState.Jump);
                    buffer.WriteFloat(AnimState.JumpLeg);
                    buffer.WriteByte((byte)((AnimState.Cast) ? 1 : 0));
                    break;
                case EntityType.NPC:
                    break;
                default:
                    break;
            }
            buffer.WriteInteger((int)_TargetType);
            buffer.WriteInteger(_TargetID);
            buffer.WriteByte(inCombat ? (byte)1 : (byte)0);
            data = null;
        }
    }
    class Spawn
    {
        public int Spawn_ID = 0;
        public float Pos_X = 0.0f;
        public float Pos_Y = 0.0f;
        public float Pos_Z = 0.0f;
        public float Rotation_Y = 0.0f;
        public int NPC_ID = -1;
        public int Collectable_ID = -1;

        public bool InUse = false;
        public int Assigned_Entity_ID = -1;

        public Spawn(int spawn_ID, float x, float y, float z, float ry, int npc, int coll)
        {
            Spawn_ID = spawn_ID;
            Pos_X = x;
            Pos_Y = y;
            Pos_Z = z;
            Rotation_Y = ry;
            NPC_ID = npc;
            Collectable_ID = coll;
        }
    }
}
