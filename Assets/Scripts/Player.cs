

using System.Collections;
using Unity.VisualScripting;

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
    [SerializeField] float searchRadius;
    [SerializeField] float polarityResetdelay;
    public AnimationManagerPlayer animatorRef;
    Coroutine resetTimerCoroutine;

    float rayDistance;
    private GameObject grabbedTile;             // reference for grabbed 
    
    //private GameObject tileToBridge;
    private int layerIndex;
    bool isPolarityTimerActive=false;                 // bool to check if reset polarity timer is active               
    Vector3 randomRepelPoint; 

    float disCheck;                  

    bool isRepelled;                            //is tile has reached the random repel point

    RaycastHit2D hitInfo;

    private float horizontal;
    private bool isFacingRight = true;          // bool to check if player facing right

    EPolarity playerPolarity;                   // player polarity 

    Rigidbody2D rb;

    Tiles HitTileRef;

    bool hasClosestSamePolTile=false;

    //Tiles TileToAttachRef;

    Vector3 collisionHandle = new Vector3(0.1f,0f,0f);

    Tiles tempClosestTile = null;

    Tiles tempClosestSameTile = null;

    GameManager gm;

    void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        gm = GameManager.instance;
        animatorRef.AddComponent<AnimationManagerPlayer>();
    }

    void Start() 
    {
        isPolarityTimerActive = false;
        isRepelled = false;
        layerIndex = LayerMask.NameToLayer("Ground");
        Debug.Log(layerIndex);
        playerPolarity = EPolarity.Negative;
        disCheck = 0f;
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
                gm.setPlayerPolarity(playerPolarity);
                animatorRef.polarityAnim(playerPolarity);
                
            }
            else if(playerPolarity==EPolarity.Negative)
            {
                setPolarity(EPolarity.Positive);
                gm.setPlayerPolarity(playerPolarity);
                animatorRef.polarityAnim(playerPolarity);
            }
            Debug.Log("Player Polarity = "+playerPolarity);

            resetTimerCoroutine = StartCoroutine(ActivatePolarity(polarityResetdelay));
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
    Vector3 placementOffset(Vector3 objectTransform)
    {
        return new Vector3((objectTransform.x/2),0f,0f);
    }

    // Attract tile to player functionality 
    void attractTile(GameObject tileToMove, Vector3 placementOffsetAdjusted, Tiles tileCompRef)
    {
        tileToMove.GetComponent<Rigidbody2D>().isKinematic = true;
        if(transform.position.x < grabPoint.position.x)
        {
            if(tileCompRef.getTileColliderType()==ETileColliderType.Box)
            tileToMove.GetComponent<BoxCollider2D>().isTrigger = true;
            else
            tileToMove.GetComponent<PolygonCollider2D>().isTrigger = true;

            tileToMove.transform.position = Vector3.MoveTowards(tileToMove.transform.position, 
                                                                grabPoint.position+placementOffset(placementOffsetAdjusted)+collisionHandle,
                                                                0.05f);
            //grabbedTile.transform.position = grabPoint.position+placementOffset(grabbedTile.transform)+new Vector3(0.1f,0f,0f);
        }
        else
        {
            if(tileCompRef.getTileColliderType()==ETileColliderType.Box)
            tileToMove.GetComponent<BoxCollider2D>().isTrigger = true;
            else
            tileToMove.GetComponent<PolygonCollider2D>().isTrigger = true;

            tileToMove.transform.position = Vector3.MoveTowards(tileToMove.transform.position, 
                                                                grabPoint.position-placementOffset(placementOffsetAdjusted)-collisionHandle,
                                                                0.05f);
            // if(grabbedTile.transform.position == grabPoint.position+placementOffset(grabbedTile.transform)-new Vector3(0.1f,0f,0f))
            //grabbedTile.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    void attractTileToStatic(GameObject tileToMove, Vector3 placementOffsetAdjusted,GameObject otherTile, Tiles tileToMoveCompRef)
    {
        tileToMove.GetComponent<Rigidbody2D>().isKinematic = true;
        if(tileToMove.transform.position.x < otherTile.gameObject.transform.position.x)
        {
            if(tileToMoveCompRef.getTileColliderType()==ETileColliderType.Box)
            tileToMove.GetComponent<BoxCollider2D>().isTrigger = true;
            else
            tileToMove.GetComponent<PolygonCollider2D>().isTrigger = true;

            tileToMove.transform.position = Vector3.MoveTowards(tileToMove.transform.position, 
                                                                otherTile.GetComponent<Tiles>().getTileAttachmentPoint().position-placementOffset(placementOffsetAdjusted)+collisionHandle,
                                                                0.05f);
            //grabbedTile.transform.position = grabPoint.position+placementOffset(grabbedTile.transform)+new Vector3(0.1f,0f,0f);
        }
        else
        {
            if(tileToMoveCompRef.getTileColliderType()==ETileColliderType.Box)
            tileToMove.GetComponent<BoxCollider2D>().isTrigger = true;
            else
            tileToMove.GetComponent<PolygonCollider2D>().isTrigger = true;

            tileToMove.transform.position = Vector3.MoveTowards(tileToMove.transform.position, 
                                                                otherTile.GetComponent<Tiles>().getTileAttachmentPoint().position+placementOffset(placementOffsetAdjusted)+collisionHandle,
                                                                0.05f);
            // if(grabbedTile.transform.position == grabPoint.position+placementOffset(grabbedTile.transform)-new Vector3(0.1f,0f,0f))
            //grabbedTile.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    bool attractTileCenter(GameObject tileToMove, Vector3 center, Tiles tileCompRef)
    {
        tileToMove.GetComponent<Rigidbody2D>().isKinematic = true;
        
            if(tileCompRef.getTileColliderType()==ETileColliderType.Box)
            tileToMove.GetComponent<BoxCollider2D>().isTrigger = true;
            else
            tileToMove.GetComponent<PolygonCollider2D>().isTrigger = true;

            tileToMove.transform.position = Vector3.MoveTowards(tileToMove.transform.position, 
                                                                center,
                                                                0.1f);

            if (Vector3.Distance(tileToMove.transform.position,center)<0.2f)
            {
                Debug.Log("RetTruexLDNALKFLA");
                return true;
            }
            else
            {
                return false;
            }
    }

    // Repel tile from player functionality
    void repelTile(GameObject tileToMove, Vector3 pointToMove)
    {
        tileToMove.GetComponent<Rigidbody2D>().isKinematic = true;
        if(tileToMove.transform.position != (transform.position+pointToMove))
        {
            isRepelled=true;
            tileToMove.transform.position = Vector3.MoveTowards(grabbedTile.transform.position, 
                                                                transform.position+pointToMove,
                                                                0.05f);
            HitTileRef.setIsRepelled(true);
        }
        else
        {
            HitTileRef.setIsRepelled(false);
            grabbedTile = null;
            isRepelled = false;
            HitTileRef = null;
        }
    }

    // reset player polarity to neutral
    void ResetPolarity()
    {
        isPolarityTimerActive = false;
        setPolarity(EPolarity.Neutral);
        GameManager.instance.setPlayerPolarity(playerPolarity);
        Debug.Log("Value reset to Neutral");
    }

    //Coroutine to activate reset player polarity timer and assign polarity to player
    IEnumerator ActivatePolarity(float resetdelay)
    {
        //setPolarity(polarity);
        isPolarityTimerActive=true;
        yield return new WaitForSeconds(resetdelay);
        if(!grabbedTile)
        ResetPolarity(); //Work later if needed

    }

    void HandleTiles()
    {
        if(grabbedTile !=null)
        {
            if(playerPolarity!=EPolarity.Neutral)
            {
                if(HitTileRef.getTilePolarity()!=playerPolarity)
                {
                    attractTile(grabbedTile, grabbedTile.GetComponent<Tiles>().getAdjustedScale(), HitTileRef);
                }
                else if(HitTileRef.getTilePolarity()==playerPolarity)
                {
                    Debug.LogWarning("Repel + "+tempClosestTile);
                    if(tempClosestTile == null && grabbedTile)
                    {   
                        Debug.LogWarning("Temp Closest tile is null");
                        tempClosestTile = findClosestOppositePolarityStaticTile();
                    }
                    if(tempClosestTile)
                    {
                        Debug.Log("Temp Closed Tile Not Null"+tempClosestTile);
                        attractTileToStatic(grabbedTile,tempClosestTile.getAdjustedScale(),tempClosestTile.GameObject(),HitTileRef);

                        Vector3 tempGrabbedTilePos = (grabbedTile.transform.position+placementOffset(HitTileRef.getAdjustedScale()));

                        Vector3 tempStaticTilePos;

                        if(grabbedTile.transform.position.x < tempClosestTile.gameObject.transform.position.x)
                        tempStaticTilePos = (tempClosestTile.gameObject.transform.position-placementOffset(tempClosestTile.getAdjustedScale()));
                        else
                        tempStaticTilePos = ((tempClosestTile.gameObject.transform.position+placementOffset(tempClosestTile.getAdjustedScale()))+tempClosestTile.getAdjustedScale());

                        disCheck = Vector3.Distance(tempGrabbedTilePos, tempStaticTilePos);

                        Debug.Log("DisCHECK = "+disCheck);
                        if(disCheck < 1 )
                        {
                            Debug.Log("Positions are Equal");

                            if(HitTileRef.getTileColliderType()==ETileColliderType.Box)
                            grabbedTile.GetComponent<BoxCollider2D>().isTrigger = false;
                            else
                            grabbedTile.GetComponent<PolygonCollider2D>().isTrigger = false;

                            //HitTileRef.setIsStatic(true);
                            grabbedTile=null;
                            Debug.LogWarning("grabbedTile Nulled");
                            tempClosestTile =null;
                            HitTileRef = null;
                            
                        }
                    }
                    else if(findClosestSamePolarityStaticTile() || hasClosestSamePolTile)
                    {
                        Debug.LogWarning("Part 2");
                        if(!hasClosestSamePolTile)
                        tempClosestSameTile=findClosestSamePolarityStaticTile();
                        
                        hasClosestSamePolTile = true;
                        //GameObject tempRef = tempClosestSameTile.gameObject;
                        GameObject partnerTile = tempClosestSameTile.getCustomTilePartner();

                        Debug.Log(partnerTile);
                        Transform tempRefTransform = tempClosestSameTile.getTileAttachmentPoint();
                        Transform partnerTransform = partnerTile.GetComponent<Tiles>().getTileAttachmentPoint();

                        Debug.Log("parttrans"+partnerTransform.transform);

                        Vector3 center = (tempRefTransform.position+partnerTransform.position)/2;
        

                        if(!attractTileCenter(grabbedTile,center,HitTileRef))
                        {
                            Debug.Log("Not Center");
                        }
                        else if(tempClosestSameTile)
                        {
                            Debug.Log("Center");
                            HitTileRef.setInitTileMove(true, tempClosestSameTile.getCustomTileType(), tempRefTransform.transform, partnerTransform.transform);
                            grabbedTile = null;
                            HitTileRef = null;
                            tempClosestSameTile = null;
                            hasClosestSamePolTile=false;
                        }

                    }
                    else if(tempClosestSameTile == null)
                    {
                        if (!isRepelled)
                        {
                            randomRepelPoint = new(Random.Range(minInclusiveRepel,maxExclusiveRepel), Random.Range(minInclusiveRepel,maxExclusiveRepel), 0f);
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
        animatorRef.setPlayerSpeed(horizontal);


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

            Debug.Log(touchPos);

            if(!grabbedTile)
            {

                Debug.Log("Grabbed Tile is NULL");
                hitInfo = Physics2D.Raycast(rayPoint.position, touchPos-rayPoint.transform.position, 10f, groundLayer);
                


                HitTileRef = hitInfo.collider.gameObject.GetComponent<Tiles>();

                Debug.Log(HitTileRef.gameObject);

                if(hitInfo)
                {
                    if(hitInfo.collider != null && hitInfo.collider.gameObject.layer == layerIndex && !HitTileRef.getIsStatic())  
                    {

                        if (HitTileRef.getTilePolarity() != EPolarity.Neutral)
                        {
                            if(HitTileRef.getTilePolarity() != playerPolarity)
                            {
                                grabbedTile = hitInfo.collider.gameObject;
                                HitTileRef.setIsGrabbed(true);
                                StopCoroutine(resetTimerCoroutine);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("No Tile Found !!");
                }
                

            }
            else
            Debug.Log("Grabbed Tile is Not Null");
        }
    }

    Tiles findClosestOppositePolarityStaticTile()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(grabbedTile.transform.position, searchRadius);
        Tiles closestTile = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in colliders)
        {
            if(collider.gameObject.layer == layerIndex)
            {
                Tiles tile = collider.GetComponent<Tiles>();
                Debug.Log("Tiles Found = "+tile.gameObject);

                if(tile.getTilePolarity() != EPolarity.Neutral)
                {
                    if (tile != null && tile.getIsStatic() && HitTileRef.getTilePolarity() != tile.getTilePolarity() && tile.getHasAttachmentPoint() )
                    {
                        float distance = Vector2.Distance(grabbedTile.transform.position, tile.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTile = tile;
                        }
                    }
                    else if (tile != null && tile.getIsStatic() && HitTileRef.getTilePolarity() == tile.getTilePolarity() && tile.getHasAttachmentPoint() )
                    {

                    }
                }
                
            }
            
        }
        Debug.Log("Closest Tile Returned = "+closestTile);
        return closestTile;
    }

    Tiles findClosestSamePolarityStaticTile()
    {
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(grabbedTile.transform.position, searchRadius);
        Tiles closestTile = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in colliders)
        {
            if(collider.gameObject.layer == layerIndex)
            {
                Tiles tile = collider.GetComponent<Tiles>();
                Debug.Log("Tiles Type 2 Found = "+tile.gameObject);

                if(tile.getTilePolarity() != EPolarity.Neutral)
                {
                    if (tile != null && tile.getIsStatic() && HitTileRef.getTilePolarity() == tile.getTilePolarity() && tile.getHasAttachmentPoint())
                    {
                        Debug.Log("TYPE 2 True");
                        float distance = Vector2.Distance(grabbedTile.transform.position, tile.transform.position);
                        if (distance < closestDistance && tile.getCustomTileType()!=ECustomTileType.None)
                        {
                            Debug.Log("TYPE 2 True 2");
                            closestDistance = distance;
                            closestTile = tile;
                        }
                    }
                }
                
            }
            
        }
        Debug.Log("Closest Tile Type 2 Returned = "+closestTile);
        return closestTile;
    }

    

}
public enum EPolarity
    {
        Positive,
        Negative,
        Neutral
    };
