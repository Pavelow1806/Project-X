using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class AnimationState
    {
        private float _Forward = 0.0f;
        public float Forward
        {
            get
            {
                return _Forward;
            }
            set
            {
                if (_Forward != value)
                {
                    Changed = true;
                    _Forward = value;
                }
            }
        }
        private float _Turn = 0.0f;
        public float Turn
        {
            get
            {
                return _Turn;
            }
            set
            {
                if (_Turn != value)
                {
                    Changed = true;
                    _Turn = value;
                }
            }
        }
        private bool _Crouch = false;
        public bool Crouch
        {
            get
            {
                return _Crouch;
            }
            set
            {
                if (_Crouch != value)
                {
                    Changed = true;
                    _Crouch = value;
                }
            }
        }
        private bool _OnGround = true;
        public bool OnGround
        {
            get
            {
                return _OnGround;
            }
            set
            {
                if (_OnGround != value)
                {
                    Changed = true;
                    _OnGround = value;
                }
            }
        }
        private float _Jump = 0.0f;
        public float Jump
        {
            get
            {
                return _Jump;
            }
            set
            {
                if (_Jump != value)
                {
                    Changed = true;
                    _Jump = value;
                }
            }
        }
        private float _JumpLeg = 0.0f;
        public float JumpLeg
        {
            get
            {
                return _JumpLeg;
            }
            set
            {
                if (_JumpLeg != value)
                {
                    Changed = true;
                    _JumpLeg = value;
                }
            }
        }
        private bool _Attacking = false;
        public bool Attacking
        {
            get
            {
                return _Attacking;
            }
            set
            {
                if (_Attacking != value)
                {
                    Changed = true;
                    _Attacking = value;
                }
            }
        }
        private bool _Dead = false;
        public bool Dead
        {
            get
            {
                return _Dead;
            }
            set
            {
                if (_Dead != value)
                {
                    Changed = true;
                    _Dead = value;
                }
            }
        }
        private bool _Attacked = false;
        public bool Attacked
        {
            get
            {
                return _Attacked;
            }
            set
            {
                if (_Attacked != value)
                {
                    Changed = true;
                    _Attacked = value;
                }
            }
        }
        private bool _Cast = false;
        public bool Cast
        {
            get
            {
                return _Cast;
            }
            set
            {
                if (_Cast != value)
                {
                    Changed = true;
                    _Cast = value;
                }
            }
        }

        public bool Changed = false;
    }
}
