using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenuAttribute(menuName = "TileType")]
public class TileData : ScriptableObject
{
    [SerializeField] Sprite sprite;
    [SerializeField] bool isStatic;
    [SerializeField] EPolarity ePolarity;
    [SerializeField] Transform playerAttachmentPoint;
    [SerializeField] Transform tileAttachmentPoint;
    [SerializeField] Vector3 collisionOffset;

    [SerializeField] Collider2D NewCollider;

}
