

using System.Collections;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] float playerSpeed;         //player speed
    [SerializeField] float jumpPower;           //jump power
    [SerializeField] Transform groundCheck;     //ground check point on Player 
    [SerializeField] LayerMask groundLayer;     //Layermask ref for ground layer
    [SerializeField] Transform grabPoint;       //transform point where player will grab the tiles
    [SerializeField] Transform rayPoint;        //Raycast Emittor point
    [SerializeField] float polarityResetTime;  //Player polarity reset time

    [SerializeField] float minInclusiveRepel;   //minimum inclusive repel distance
    [SerializeField] float maxExclusiveRepel;   //maximum exclusive repel distance

    float rayDistance;
    private GameObject grabbedTile;             // reference for grabbed 
    
    private GameObject tileToBridge;
    private int layerIndex;
    bool isPolarityTimerActive;                 // bool to check if reset polarity timer is active               
    Vector3 randomRepelPoint;                   

    bool isRepelled;                            //is tile has reached the random repel point

    private float horizontal;
    private bool isFacingRight = true;          // bool to check if player facing right

    EPolarity playerPolarity;                   // player polarity 

    Rigidbody2D rb;

    Tiles HitTileRef;

    Tiles TileToAttachRef;

    Vector3 collisionHandle = new Vector3(0.1f,0f,0f);

    void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() 
    {
        isPolarityTimerActive = false;
        isRepelled = false;
        layerIndex = LayerMask.NameToLayer("Ground");
        Debug.Log(layerIndex);
        playerPolarity = EPolarity.Negative;
    }

    //Function to Check if Player is Ground
    private bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.4f, groundLayer);
    }
    //Function to Flip the player
    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight=!isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }   

    // Update is called once per frame
    void Update()
    {   

        HandleTiles();

        HandleMovement();
        
        GrabTiles();
        

        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(playerPolarity==EPolarity.Positive || playerPolarity==EPolarity.Neutral)
            {
                setPolarity(EPolarity.Negative);
            }
            else if(playerPolarity==EPolarity.Negative)
            {
                setPolarity(EPolarity.Positive);
            }
            Debug.Log(playerPolarity);
        }
    }

    private void FixedUpdate() 
    {
        rb.velocity = new Vector2(horizontal * playerSpeed, rb.velocity.y);
    }

    //Function for Fetching Player Polarity
    public EPolarity GetEPolarity()
    {
        return playerPolarity;
    }

    //Function for Setting/Updating Player Polarity
    public void setPolarity(EPolarity newPolarity)
    {
        playerPolarity = newPolarity;
    }

    // Get Tile position for calculating direction of tile in context to player position
    Vector3 getTouchPosition()
    {
        
        Vector3 touchPostion = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        touchPostion.z=0f;

        return touchPostion;
    }

    // calculate edge offset of the tile to attach to player
    Vector3 placementOffset(Transform objectTransform)
    {
        return new Vector3((objectTransform.localScale.x/2),0f,0f);
    }

    // Attract tile to player functionality 
    void attractTile(GameObject tileToMove)
    {
        tileToMove.GetComponent<Rigidbody2D>().isKinematic = true;
        if(transform.position.x < grabPoint.position.x)
        {
            grabbedTile.GetComponent<BoxCollider2D>().isTrigger = true;
            grabbedTile.transform.position = Vector3.MoveTowards(grabbedTile.transform.position, 
                                                                grabPoint.position+placementOffset(grabbedTile.transform)+collisionHandle,
                                                                0.05f);
            //grabbedTile.transform.position = grabPoint.position+placementOffset(grabbedTile.transform)+new Vector3(0.1f,0f,0f);
        }
        else
        {
            grabbedTile.GetComponent<BoxCollider2D>().isTrigger = true;
            grabbedTile.transform.position = Vector3.MoveTowards(grabbedTile.transform.position, 
                                                                grabPoint.position-placementOffset(grabbedTile.transform)-collisionHandle,
                                                                0.05f);
            // if(grabbedTile.transform.position == grabPoint.position+placementOffset(grabbedTile.transform)-new Vector3(0.1f,0f,0f))
            //grabbedTile.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    // Repel tile from player functionality
    void repelTile(GameObject tileToMove, Vector3 pointToMove)
    {
        tileToMove.GetComponent<Rigidbody2D>().isKinematic = true;
        if(grabbedTile.transform.position != (transform.position+pointToMove))
        {
            isRepelled=true;
            grabbedTile.transform.position = Vector3.MoveTowards(grabbedTile.transform.position, 
                                                                transform.position+pointToMove,
                                                                0.05f);
        }
        else
        {
            grabbedTile = null;
            isRepelled = false;
        }
    }

    // reset player polarity to neutral
    void ResetPolarity()
    {
        isPolarityTimerActive = false;
        setPolarity(EPolarity.Neutral);
        Debug.Log("Value reset to Neutral");
    }

    //Coroutine to activate reset player polarity timer and assign polarity to player
    IEnumerator ActivatePolarity(float resetdelay)
    {
        //setPolarity(polarity);
        isPolarityTimerActive=true;
        yield return new WaitForSeconds(resetdelay);
        ResetPolarity(); //Work later if needed

    }
    Transform getPositionOfAttachment()
    {
        // Transform Position1 = grabbedTile.transform.Find("position1");
        // Transform Position2 = grabbedTile.transform.Find("position2");
        return transform;
        
    }

    void attachObjects(GameObject grabbedObj, GameObject objToAttach)
    {
        Bounds boundGrabbedObj = grabbedObj.GetComponent<Renderer> ().bounds;
        Bounds boundObjToAttach = objToAttach.GetComponent<Renderer>().bounds;

        float GrabbedObjLeft = boundGrabbedObj.min.x;
        float AttachObjRight = boundGrabbedObj.max.x;
    }

    void HandleTiles()
    {
        if(grabbedTile !=null)
        {
            if(playerPolarity!=EPolarity.Neutral)
            {
                if(HitTileRef.getTilePolarity()!=playerPolarity)
                {
                    attractTile(grabbedTile);
                }
                else if(HitTileRef.getTilePolarity()==playerPolarity)
                {
                    if(HitTileRef.gettIsAvailableForAttachment())
                    {
                        
                    }
                    else
                    {
                        if (!isRepelled)
                        {
                            randomRepelPoint = new(Random.Range(3f,6f), Random.Range(3f,6f), 0f);
                            Debug.Log("Random Location of Repel : "+randomRepelPoint);
                        }
                        repelTile(grabbedTile, randomRepelPoint);
                    }
                    
                    

                }
            }
            else
            {

            }

        }
    }

    void HandleMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        Flip();

        if(Input.GetButtonDown("Jump") && isGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        }
    }

    void GrabTiles()
    {
        if(Input.GetMouseButtonDown(0))
        {   
            Vector3 touchPos = getTouchPosition();
            if(!grabbedTile)
            {
                
                RaycastHit2D hitInfo = Physics2D.Raycast(rayPoint.position, touchPos-rayPoint.transform.position, 10f, groundLayer);
                
                HitTileRef = hitInfo.collider.gameObject.GetComponent<Tiles>();

                if(hitInfo.collider != null && hitInfo.collider.gameObject.layer == layerIndex && !HitTileRef.getIsStatic())  
                {

                    if (HitTileRef.getTilePolarity() != EPolarity.Neutral)
                    {
                        if(HitTileRef.getTilePolarity() != playerPolarity)
                        {
                            grabbedTile = hitInfo.collider.gameObject;
                            //HitTileRef.setIsGrabbed(true);
                        }
                    }
                }

            }
            // else if (grabbedTile)
            // {
            //     RaycastHit2D hitInfo = Physics2D.Raycast(rayPoint.position, touchPos-rayPoint.transform.position, 10f, groundLayer);
                
            //     HitTileRef = hitInfo.collider.gameObject.GetComponent<Tiles>();

            //     if(hitInfo.collider != null && hitInfo.collider.gameObject.layer == layerIndex) 
            //     {
            //         if(HitTileRef.getTilePolarity() != playerPolarity && !HitTileRef.getIsStatic())
            //         {
            //             tileToBridge = hitInfo.collider.gameObject;
            //         }
            //     }


            // }
        }
    }

}

public enum EPolarity
    {
        Positive,
        Negative,
        Neutral
    };
