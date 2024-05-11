
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tiles : MonoBehaviour
{
    [SerializeField] EPolarity tilePolarity;
    [SerializeField] bool isStatic;
    [SerializeField] Transform playerAttachmentPoint;
    [SerializeField] Transform tileAttachmentPoint;
    
    [SerializeField] Vector3 collisionOffset;

    BoxCollider2D tileCollider;
    Rigidbody2D rb;

    [SerializeField] TileData data;



    public Tiles TileAvailable;

    bool isAttachedSomewhere;

    //bool isGrabbed;

    bool availableForAttachment;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tileCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        if(!isStatic)
        isAttachedSomewhere = false;
        else
        isAttachedSomewhere = true;
        collisionOffset = new Vector3(0.1f,0.1f,0);
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

    // public void setIsAvailableForAttachment(bool available)
    // {
    //     availableForAttachment = available;
    // }

    public bool gettIsAvailableForAttachment()
    {
        return availableForAttachment;
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
        rb.isKinematic = true;
        if (transform.position != (transform.position + pointToMove))
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                      transform.position + pointToMove,
                                                      0.05f);
        }
    }

    // Calculate edge offset of the tile to attach to player
    private Vector3 PlacementOffset(Transform objectTransform)
    {
        return new Vector3((objectTransform.localScale.x / 2), 0f, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        Tiles otherTileRef = other.gameObject.GetComponent<Tiles>();
        if(otherTileRef.getTilePolarity() != tilePolarity && !otherTileRef.getIsStatic() && !isAttachedSomewhere)
        {
            availableForAttachment = true;
            TileAvailable = otherTileRef;
            // Renderer renderer = other.gameObject.GetComponentInParent<Renderer>();
            // if (renderer != null)
            // {
            //     Bounds bounds = renderer.bounds;
            //     Vector3 positionOfAttach = new Vector3(bounds.min.x, bounds.center.y, 0f);

                

            // }

            Transform child = TileAvailable.transform.Find("AnchorPoint");

            while( transform != child )
                {
                    AttractTile(child,collisionOffset);
                }
             

        }       
    }
}
