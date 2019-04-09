using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public enum EntityType
    {
        Player,
        NPC
    }
    class Entity
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

        public AnimationState AnimState = new AnimationState();

        protected bool Changed = false;

        protected ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

        public Entity(int ID, string name, int level, float x, float y, float z, float r)
        {
            Entity_ID = World.instance.EntityCounter;
            _Name = name;
            _Level = level;
            _x = x;
            _y = y;
            _z = z;
            _r = r;
        }

        protected virtual void BuildTransmission(out byte[] data)
        {
            buffer = null;
            buffer.WriteInteger((int)type);
            buffer.WriteInteger(Entity_ID);
            buffer.WriteString(Name);
            buffer.WriteInteger(Level);
            buffer.WriteFloat(_x);
            buffer.WriteFloat(_y);
            buffer.WriteFloat(_z);
            buffer.WriteFloat(_r);
            buffer.WriteByte((byte)((AnimState.Attacking) ? 1 : 0));
            buffer.WriteByte((byte)((AnimState.Attacked) ? 1 : 0));
            buffer.WriteByte((byte)((AnimState.Dead) ? 1 : 0));
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
            data = null;
        }
    }
}
