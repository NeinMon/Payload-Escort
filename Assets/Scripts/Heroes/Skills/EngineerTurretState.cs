using UnityEngine;

public class EngineerTurretState : MonoBehaviour
{
    public GameObject LastTurret { get; private set; }
    public bool HasActiveTurret => LastTurret != null;

    public void SetTurret(GameObject turret)
    {
        LastTurret = turret;
    }

    public void ClearTurret()
    {
        LastTurret = null;
    }
}
