using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public GameObject[] grenades;
    public bool[] hasweapons;
    public int hasGrenades;

    public int ammo;
    public int coin;
    public int health;

    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;


    float hAxis;
    float vAxis;

    bool iDown; //weapon
    bool wDown;
    bool fDown;
    bool jDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;


    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isFireReady = true;

    Vector3 movevec;
    Vector3 dodgevec;

    Animator animator;
    Rigidbody rigid;

    GameObject nearobject;
    Weapon equipWeapon; //장착중인 무기
    int equipWeaponIndex= -1;
    float fireDelay;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Dodge();
        interation();
        Swap();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); //GetAxisRaw : -1, 0 ,1 
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        iDown = Input.GetButtonDown("interation");
        sDown1 = Input.GetButtonDown("swap1");
        sDown2 = Input.GetButtonDown("swap2");
        sDown3 = Input.GetButtonDown("swap3");
    }

    void Move()
    {
        movevec = new Vector3(hAxis, 0, vAxis).normalized; 

        if(isDodge) 
        {
            movevec = dodgevec;
        }

        if(isSwap || !isFireReady)
        {
            movevec = Vector3.zero;
        }

        transform.position += movevec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime; 

        animator.SetBool("Isrun", movevec != Vector3.zero); 
        animator.SetBool("Iswalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + movevec);
    }

    void Jump()
    {
        if(jDown && movevec==Vector3.zero && !isJump && !isDodge &&!isSwap)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 20, ForceMode.Impulse);
            animator.SetBool("Isjump", true);
            animator.SetTrigger("Dojump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && movevec != Vector3.zero && !isJump && !isDodge && !isSwap) 
        {
            dodgevec = movevec; 
            animator.SetTrigger("Dododge");
            speed *= 2;
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void interation()
    {
        if(iDown && nearobject !=null && !isJump && !isDodge)
        {
            if(nearobject.tag=="Weapon")
            {
                Item item = nearobject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasweapons[weaponIndex] = true;

                Destroy(nearobject);
            }
        }
    }

    void Swap()
    {
        if (sDown1 && (!hasweapons[0] || equipWeaponIndex == 0)) return; //획득하지 않은 무기거나, 이미 들고있는 무기면 return
        if (sDown2 && (!hasweapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasweapons[2] || equipWeaponIndex == 2)) return;


        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if(equipWeapon!=null)
            {
                equipWeapon.gameObject.SetActive(false);
            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("Doswap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Attack()
    {
        if (equipWeapon == null) return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type==Weapon.Type.Melee ? "Doswing" : "Doshot");
            fireDelay = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Floor")
        {
            animator.SetBool("Isjump", false);
            isJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if( ammo >maxammo)
                    {
                        ammo = maxammo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                    {
                        coin = maxcoin;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxhealth)
                    {
                        health=maxhealth;
                    }
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxhasGrenades)
                    {
                        hasGrenades = maxhasGrenades;
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag=="Weapon")
        {
            nearobject = other.gameObject;
            Debug.Log(nearobject.name);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearobject = null;
        }
    }
}
