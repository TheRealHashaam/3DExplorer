using Cinemachine;
using DG.Tweening;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class ShootingMechanic : MonoBehaviour
{
    public GameObject Gun;
    public Animator animator;
    bool _canequip = true;
    public float initialTime = 5f;
    private float currentTime;
    private bool timerRunning = false;
    bool _canfire = false;
    bool _canAim= false;
    bool _isAiming = false;
    [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
    public CinemachineImpulseSource impulse;
    public float desiredRotationSpeed = 1f;
    public StarterAssetsInputs starterAssetsInputs;
    public IKControl ikControl;
    public ParticleSystem Shoot;
    public Image Crosshair;
    public float duration;
    public Transform FirePos, AimPos;
    public float FirePosDelay, DefaultPosDelay;
    public float rotationOffset = 45.0f;
    public GameObject BloodParticle;
    public AudioSource FireSound;
    void Start()
    {
        currentTime = initialTime;
    }
    void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                HandleTimerEnd();
            }
        }
        if(!_canequip)
        {
            AimState();
        }
    }
    public void CrossHairfade(float val)
    {
        Crosshair.DOFade(val, duration);
    }
    public void EnableIK()
    {
        ikControl.ikActive = true;
    }
    public void DisableIK()
    {
        ikControl.ikActive = false;
    }
    void AimState()
    {
        if (starterAssetsInputs.aim && _canAim)
        {
            EnableIK();
            timerRunning = false;
            ResetTimer();
            _aimVirtualCamera.gameObject.SetActive(true);
            animator.SetBool("Aim", true);
            //Vector3 targetRotation;
            Quaternion targetRotation;
            
            targetRotation = Quaternion.LookRotation(Camera.main.transform.forward + (Camera.main.transform.right * 0.1f)) * Quaternion.Euler(0, rotationOffset, 0);
            float f;
            f = Camera.main.transform.localEulerAngles.x;
            if (f > 180)
            {
                f -= 360;
            }
            f = Mathf.Clamp(f, -10, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z), desiredRotationSpeed);
            CrossHairfade(1);
        }
        else
        {
            DisableIK();
            _aimVirtualCamera.gameObject.SetActive(false);
            timerRunning = true;
            CrossHairfade(0);
            transform.DORotate(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z), .2f);
            animator.SetBool("Aim", false);
        }
    }
    

    void HandleTimerEnd()
    {
        timerRunning = false;
        Debug.Log("Timer reached zero!");
        ResetTimer();
        Unequip();
    }

    void ResetTimer()
    {
        currentTime = initialTime;
    }
    public void OnEquip()
    {
        if(_canequip)
        {
            animator.SetTrigger("Equip");
            _canequip = false;
            timerRunning = true;
            _canfire = true;
            _canAim = true;
            ResetTimer();
        }
    }
    public void EnableGunObject()
    {
        Gun.SetActive(true);
    }
    public void DisableGunObject()
    {
        Gun.SetActive(false);
    }
    void Unequip()
    {
        if (!_canequip)
        {
            animator.SetTrigger("UnequipNow");
            _canequip = true;
            timerRunning = false;
            _canfire = false;
            ResetTimer();
        }
    }
    public void OnFire()
    {
        Fire();
    }
    void Fire()
    {
        if(_canfire)
        {
            _canfire = false;
            Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0.0f);
            Ray ray = Camera.main.ViewportPointToRay(screenCenter);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponentInParent<EnemyAi>())
                {
                    EnemyAi enemy = hit.collider.GetComponentInParent<EnemyAi>();
                    enemy.TakeDamage();
                    Instantiate(BloodParticle, hit.point, Quaternion.LookRotation(hit.normal), hit.transform);
                }
            }
            ikControl.rightHandObj.DOLocalMove(FirePos.localPosition, FirePosDelay).OnComplete(() =>
            {
                ikControl.rightHandObj.DOLocalMove(AimPos.localPosition, DefaultPosDelay).OnComplete(() =>
                {
                    _canfire = true;
                });
                ikControl.rightHandObj.DOLocalRotateQuaternion(AimPos.localRotation, DefaultPosDelay);
            });
            ikControl.rightHandObj.DOLocalRotateQuaternion(FirePos.localRotation, FirePosDelay);
            impulse.GenerateImpulse();
            animator.SetTrigger("Fire");
            ResetTimer();
            Shoot.Play();
            FireSound.Play();
        }
    }
}