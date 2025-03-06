using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{

    private Rigidbody rbPlayer;

    private Vector3 direction = Vector3.zero;
    [SerializeField]

    private float forceMultiplier = 15.0f;
    [SerializeField]

    private ForceMode forceMode;
    public GameObject spawnpoint;

    private Dictionary<Item.VegetableType, int> inventory = new Dictionary<Item.VegetableType, int>();

    // Start is called before the first frame update
    void Start()
    {
        rbPlayer = GetComponent<Rigidbody>();

        foreach (Item.VegetableType type in System.Enum.GetValues(typeof(Item.VegetableType)))
        {
            inventory.Add(type, 0);
        }
    }

     void Update()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        float horizontalVelocity = Input.GetAxis("Horizontal");
        float verticalVelocity = Input.GetAxis("Vertical");

        direction = new Vector3(horizontalVelocity, 0, verticalVelocity);
    }


    void FixedUpdate()
    {
        if (!IsLocalPlayer)
        {
            return;
        }
        if (IsServer)
        {
            Move(direction);
        }
        else
        {
            MoveRpc(direction);
        }
    }
    private void Move(Vector3 input)
    {
        rbPlayer.AddForce(direction * forceMultiplier, forceMode);

        if (transform.position.z > 38)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 38);
        }
        else if (transform.position.z < -38)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -38);
        }
    }
    [Rpc(SendTo.Server)]
    public void MoveRpc(Vector3 input)
    {
        Move(input);
    }
    private void Respawn()
    {
        transform.position = spawnpoint.transform.position;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!IsLocalPlayer)
        {
            return;
        }
        if (collider.CompareTag("Item"))
        {
            Item item = collider.gameObject.GetComponent<Item>();
            AddItemToInventory(item);
        }
    }

    private void AddItemToInventory(Item item)
    {
        inventory[item.typeOfVeggie]++;

    }

    private void PrintInventory()
    {
        string output = "";

        foreach (KeyValuePair<Item.VegetableType, int> pair in inventory)
        {
            output += string.Format("{0}: {1}", pair.Key, pair.Value);
        }

        Debug.Log(output);
    }

    void OnTriggerExit(Collider collider)
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (collider.CompareTag("Hazard"))
        {
            Respawn();
        }
    }
}
