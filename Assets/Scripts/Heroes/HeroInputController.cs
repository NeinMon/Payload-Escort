using UnityEngine;
using Photon.Pun;

public class HeroInputController : MonoBehaviour
{
    [SerializeField] private HeroRuntime heroRuntime;

    [Header("Key Bindings")]
    public KeyCode skillQ = KeyCode.Q;
    public KeyCode skillE = KeyCode.E;
    public KeyCode skillR = KeyCode.X;
    public KeyCode skillF = KeyCode.F;

    private PhotonView photonView;
    private IHeroHoldSkill holdQ;
    private IHeroHoldSkill holdE;

    void Awake()
    {
        if (heroRuntime == null)
            heroRuntime = GetComponent<HeroRuntime>();

        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        RefreshHoldSkills();
        if (heroRuntime != null)
            heroRuntime.HeroChanged += RefreshHoldSkills;
    }

    void OnDestroy()
    {
        if (heroRuntime != null)
            heroRuntime.HeroChanged -= RefreshHoldSkills;
    }

    void Update()
    {
        if (heroRuntime == null) return;

        if (photonView != null && !photonView.IsMine)
            return;

        if (Input.GetKeyDown(skillQ))
        {
            if (holdQ != null)
                holdQ.BeginHold(heroRuntime);
            else
                heroRuntime.TryActivateSkill(HeroSkillSlot.Q);
        }

        if (Input.GetKey(skillQ) && holdQ != null)
            holdQ.UpdateHold(heroRuntime);

        if (Input.GetKeyUp(skillQ) && holdQ != null)
            holdQ.EndHold(heroRuntime);

        if (Input.GetKeyDown(skillE))
        {
            if (holdE != null)
                holdE.BeginHold(heroRuntime);
            else
                heroRuntime.TryActivateSkill(HeroSkillSlot.E);
        }

        if (Input.GetKey(skillE) && holdE != null)
            holdE.UpdateHold(heroRuntime);

        if (Input.GetKeyUp(skillE) && holdE != null)
            holdE.EndHold(heroRuntime);

        if (Input.GetKeyDown(skillR))
            heroRuntime.TryActivateSkill(HeroSkillSlot.R);

        if (Input.GetKeyDown(skillF))
            heroRuntime.TryActivateSkill(HeroSkillSlot.F);
    }

    private void RefreshHoldSkills()
    {
        if (heroRuntime == null) return;

        holdQ = heroRuntime.GetBehaviour(HeroSkillSlot.Q) as IHeroHoldSkill;
        holdE = heroRuntime.GetBehaviour(HeroSkillSlot.E) as IHeroHoldSkill;
    }
}
