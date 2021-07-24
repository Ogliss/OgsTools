using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CompTurret
{
    // CompTurret.ApparelTurret
    class ApparelTurret : Apparel
    {
        private List<CompTurret> turrets;
        public List<CompTurret> Turrets
        {
            get
            {
                if (turrets.NullOrEmpty())
                {
                    if (turrets == null)
                    {
                        turrets = new List<CompTurret>();
                        for (int i = 0; i < this.AllComps.Count; i++)
                        {
                            CompTurret t = this.AllComps[i] as CompTurret;
                            if (t != null)
                            {
                                turrets.Add(t);
                            }
                        }
                    }
                }
                return turrets;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Wearer != null)
            {
                for (int i = 0; i < Turrets.Count; i++)
                {
                    CompTurretGun turretGun = Turrets[i] as CompTurretGun;
                    if (turretGun != null)
                    {
                        turretGun.CompTick();
                    }
                }
            }
        }
    }
}
