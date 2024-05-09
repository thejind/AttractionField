using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    [SerializeField] EPolarity tilePolarity;
    [SerializeField] bool isStatic;
    [SerializeField] EAttachmentType attachmentType;
    [SerializeField] CircleCollider2D outerCollider;


    [SerializeField] Transform TileAttachmentPointA;
    [SerializeField] Transform TileAttachmentPointB;

    BoxCollider2D tileCollider;
    Rigidbody2D rb;

    GameObject EngagedObject;

    Tiles EngagedObjectTileRef;

    bool isGrabbedByUser;

    bool isRepelledByUserAfterGrab;

    bool isStable;

    public bool repelledTile;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tileCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        isStable=false;
        isGrabbedByUser = false;
        isRepelledByUserAfterGrab = false;
        repelledTile = false;
    }

    public EPolarity getTilePolarity()
    {
        return tilePolarity;
    }

    public void setTilePolarity(EPolarity newPolarity)
    {
        tilePolarity = newPolarity;
    }

    public bool getIsStatic()
    {
        return isStatic;
    }

    public EAttachmentType getAttachmentType()
    {
        return attachmentType;
    }

    public void setIsGrabbedByUser(bool isGrabbed)
    {
        isGrabbedByUser=isGrabbed;
    }

    public void setIsRepelledByUserAfterGrab(bool isGrabbed)
    {
        isRepelledByUserAfterGrab=isGrabbed;
    }

    public bool getIsRepelledByUserAfterGrab()
    {
        return isRepelledByUserAfterGrab;
    }

    // Attract tile to player functionality 
    public void AttractTile(Transform grabPoint, Vector3 collisionHandle)
    {
        rb.isKinematic = true;
        if (transform.position.x < grabPoint.position.x)
        {
            tileCollider.isTrigger = true;
            transform.position = Vector3.MoveTowards(transform.position,
                                                      grabPoint.position + PlacementOffset(transform) + collisionHandle,
                                                      0.05f);
        }
        else
        {
            tileCollider.isTrigger = true;
            transform.position = Vector3.MoveTowards(transform.position,
                                                      grabPoint.position - PlacementOffset(transform) - collisionHandle,
                                                      0.05f);
        }
    }

    // Repel tile from player functionality
    public void RepelTile(Vector3 pointToMove)
    {
        //Vector3 pointToMove = new (0f,0f,0f);
        // if(!repelledTile)
        // {
        //     pointToMove = new (Random.Range(3f,6f), Random.Range(3f,6f), 0f);
        // }
        rb.isKinematic = true;
        if (transform.position != (transform.position + pointToMove))
        {
            repelledTile = true;
            transform.position = Vector3.MoveTowards(transform.position,
                                                      transform.position + pointToMove,
                                                      0.05f);
        }
        else
        {
            repelledTile = false;
        }
    }

    // Calculate edge offset of the tile to attach to player
    private Vector3 PlacementOffset(Transform objectTransform)
    {
        return new Vector3((objectTransform.localScale.x / 2), 0f, 0f);
    }

    // private void OnCollisionEnter2D(Collision2D other) 
    // {
    //     if(tilePolarity!=EPolarity.Neutral)
    //     {
    //         if(other.collider == GetComponent<CircleCollider2D>())
    //         {
    //             EngagedObject = other.gameObject;
    //             EngagedObjectTileRef = other.gameObject.GetComponent<Tiles>();

    //             if(EngagedObjectTileRef.getTilePolarity()!=EPolarity.Neutral || !EngagedObjectTileRef.getIsStatic())
    //             {
    //                 if(EngagedObjectTileRef.getTilePolarity()!=tilePolarity && EngagedObjectTileRef.getAttachmentType()!=attachmentType)
    //                 {
    //                     isStable = true;

    //                 }
    //             }

    //         }
    //     }

       
    // }

    private void OnTriggerEnter2D(Collider2D other) 
    {

        Debug.Log("On Collision Enter");
        if(isRepelledByUserAfterGrab)
        {
            if(other == GetComponent<CircleCollider2D>())
            {
                EngagedObject = other.gameObject;
                EngagedObjectTileRef = other.gameObject.GetComponent<Tiles>();

                if(EngagedObjectTileRef.getTilePolarity()!=EPolarity.Neutral || !EngagedObjectTileRef.getIsStatic())
                {
                    if(EngagedObjectTileRef.getTilePolarity()!=tilePolarity && EngagedObjectTileRef.getAttachmentType()!=attachmentType)
                    {
                        isStable = true;
                        Debug.Log("is stable = true");

                    }
                    else if(EngagedObjectTileRef.getTilePolarity()!=tilePolarity && EngagedObjectTileRef.getAttachmentType()==attachmentType)
                    {
                        isStable = false;
                        Debug.Log("is stable = false");
                    }
                }

            }
        }
        
    }

}

public enum EAttachmentType
{
    Male,
    Female
}
