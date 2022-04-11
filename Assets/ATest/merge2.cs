using UnityEngine;
 using System.Collections;
 using System.Collections.Generic;
 using System.IO;
 
public class merge2 : MonoBehaviour
{
     // Use this for initialization
	void Start()
	{
		Combine(transform);
	}

	    // Update is called once per frame
	void Update()
	{
	}

	 
	public Transform Combine(Transform root)
	{
		 float startTime = Time.realtimeSinceStartup;
	 
	     // The SkinnedMeshRenderers that will make up a character will be
		 List<CombineInstance> combineInstances = new List<CombineInstance>();
         List<Material> materials = new List<Material>();
         Material material = null;
         List<Transform> bones = new List<Transform>();
         Transform[] transforms = root.GetComponentsInChildren<Transform>();
         List<Texture2D> textures = new List<Texture2D>();
         int width = 0;
         int height = 0;
 
	     int uvCount = 0;
 
	     List<Vector2[]> uvList = new List<Vector2[]>();

		foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>())
 		{
            if (material == null)
	        material = Instantiate(smr.sharedMaterial) as Material;
          	for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
            {
                 CombineInstance ci = new CombineInstance();
                 ci.mesh = smr.sharedMesh;
                 ci.subMeshIndex = sub;
                 combineInstances.Add(ci);
             }
 
	         uvList.Add(smr.sharedMesh.uv);
             uvCount += smr.sharedMesh.uv.Length;
 
             if (smr.material.mainTexture != null)
             {
                 textures.Add(smr.GetComponent<Renderer>().material.mainTexture as Texture2D);
                 width += smr.GetComponent<Renderer>().material.mainTexture.width;
                 height += smr.GetComponent<Renderer>().material.mainTexture.height;
             }
 
	             // we need to recollect references to the bones we are using
             foreach (Transform bone in smr.bones)
             {
                 foreach (Transform transform in transforms)
                 {
	                     if (transform.name != bone.name) continue;
	                     bones.Add(transform);
	                     break;
                 }
		     }
             Object.Destroy(smr.gameObject);
         }
	 
 		// Obtain and configure the SkinnedMeshRenderer attached to
        // the character base.
		SkinnedMeshRenderer r = root.gameObject.GetComponent<SkinnedMeshRenderer>();
	    if (!r)
			r = root.gameObject.AddComponent<SkinnedMeshRenderer>();
	      
		r.sharedMesh = new Mesh();
	 
		//only set mergeSubMeshes true will combine meshs into single submesh
		r.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        r.bones = bones.ToArray();
        r.material = material;
        SSTimer ss = new SSTimer("合图PackTextures方式");
     	Texture2D skinnedMeshAtlas = new Texture2D(1024, 1024);
 		Rect[] packingResult = skinnedMeshAtlas.PackTextures(textures.ToArray(), 0);
        ss.Dispose();
 		Vector2[] atlasUVs = new Vector2[uvCount];
	 
		//as combine textures into single texture,so need recalculate uvs
			 
		int j = 0;        
        for (int i = 0; i < uvList.Count; i++)
        {
	    	foreach (Vector2 uv in uvList[i])
	        {
                 atlasUVs[j].x = Mathf.Lerp(packingResult[i].xMin, packingResult[i].xMax, uv.x);
                 atlasUVs[j].y = Mathf.Lerp(packingResult[i].yMin, packingResult[i].yMax, uv.y);

                 //Debug.Log("第" + i+1 + "个矩形" + "xMin == " + packingResult[i].xMin + "   xMax == " + packingResult[i].xMax+"  原始uvX =="+uv.x+
                 //    "  合并后 == " + atlasUVs[j].x + "yMin == " + packingResult[i].yMin + "   yMax == " + packingResult[i].yMax + "  原始uvY ==" 
                 //    + uv.y + "  合并后 == " + atlasUVs[j].y);
                 j++;
		    }
	     }
	         
		 r.material.mainTexture = skinnedMeshAtlas;
	     r.sharedMesh.uv = atlasUVs;
	 
		 
	     return root;
	 }
 }