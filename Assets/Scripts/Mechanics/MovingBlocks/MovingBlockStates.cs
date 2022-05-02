﻿using System.Numerics;

namespace Mechanics.MovingBlocks {
    public class Idle : MovingBlockState {
        public override void Enter(MovingBlockInput i) {
            MySM.M.Stop();
        }

        public override void FixedUpdate() {
            
        }
        
        public override void HitWall() {
            // MySM.Transition<Zooming>();
        }
    }
    
    public class Zooming : MovingBlockState {
        public override void Enter(MovingBlockInput i) {
            MySM.M.Zoom();
        }

        public override void FixedUpdate() {
            if (MySM.M.PositionedAtZoomEnd()) {
                MySM.Transition<Returning>();
            }
            // MySM.M.Decel();
        }

        public override void HitWall() {
            MySM.Transition<Returning>();
        }
    }
    
    public class Returning : MovingBlockState {
        public override void FixedUpdate() {
            MySM.M.Decel();
            if (MySM.M.PositionedAtStart()) {
                MySM.Transition<Idle>();
            }
        }
        
        public override void HitWall() {
            MySM.Transition<Idle>();
        }
    }
}