using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureCorrecter : MonoBehaviour
{

    public float texScale = 0.5f;



    private void Start()
    {
        Renderer render = GetComponent<Renderer>();
        render.material.mainTextureScale = new Vector2(texScale, texScale);
    }

}
