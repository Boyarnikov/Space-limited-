using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    [SerializeField] MainSpriteBehaviour sprite_behaviour;
    [SerializeField] SpriteRenderer sprite_renderer;
    [SerializeField] List<Vector2> positions;
    [SerializeField] Sprite[] sprites;
    void Start()
    {
        SetInactive();
        Disable();
        Vector2 sprite_size = new Vector2(sprite_renderer.bounds.size.x, sprite_renderer.bounds.size.y);

        float w = sprite_size.x;
        float h = sprite_size.y;
        float cam_w = Camera.main.orthographicSize * Camera.main.aspect;
        float cam_h = Camera.main.orthographicSize;
        float cam_x = Camera.main.transform.position.x;
        float cam_y = Camera.main.transform.position.y;

        positions.Add(new Vector2(0, 0));
        positions.Add(new Vector2(cam_w - w * transform.localScale.x - 0.5f + cam_x, - cam_h + h * transform.localScale.y - 1f + cam_y));
        positions.Add(new Vector2(- cam_w + w * transform.localScale.x + 0.5f + cam_x, - cam_h + h * transform.localScale.y - 1f + cam_y));
    }

    public void Enable() {
        sprite_behaviour.Enable();
    }

    public void Disable()
    {
        sprite_behaviour.Disable();
    }

    public void SetPostion(ActorPosition actor_position, int actor_state)
    {
        if (actor_position == ActorPosition.FIXED) { return; }

        transform.position = positions[(int)actor_position];
        if (actor_position == ActorPosition.LEFT) sprite_renderer.flipX = true;
        if (actor_position == ActorPosition.RIGHT) sprite_renderer.flipX = false;
        if (actor_state >= 0)
        {
            sprite_renderer.sprite = sprites[actor_state];
        }
    }

    public void SetActive()
    {
        Enable();
        sprite_behaviour.SetMainColor(Color.white);
    }

    public void SetInactive()
    {
        Enable();
        sprite_behaviour.SetMainColor(Color.gray);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
