using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peposltlon : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;

        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;
        float diffX = Mathf.Abs(playerPos.x - myPos.x);
        float diffY = Mathf.Abs(playerPos.y - myPos.y);

        Vector3 playerDir = GameManager.instance.player.inputVec;
        float dirX =playerDir.x < 0 ? -1 : 1;
        float difY =playerDir.y < 0 ? -1 : 1;

        switch(transform.tag)
        {
            case "Ground":
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 40);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * difY * 40);
                }
                else
                {
                    transform.Translate(Vector3.right * dirX * 40);
                    transform.Translate(Vector3.up * difY * 40);
                }
                break;
            case "Enemy":
                break ;
        }
        


    }
}
