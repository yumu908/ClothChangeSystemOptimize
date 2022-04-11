using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class merge : MonoBehaviour
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

        int xyMax = 1024;

        Texture2D skinnedMeshAtlas = new Texture2D(xyMax, xyMax);
        // 		Rect[] rec = skinnedMeshAtlas.PackTextures(textures.ToArray(), 0);
        SSTimer ss = new SSTimer("合图raw方式");
        Rect[] rec = new Rect[3];
        rec[0].xMin = 0; rec[0].xMax = 0.5f; rec[0].yMin = 0; rec[0].yMax = 0.5f;
        rec[1].xMin = 0.5f; rec[1].xMax = 0.75f; rec[1].yMin = 0f; rec[1].yMax = 0.25f;
        rec[2].xMin = 0.75f; rec[2].xMax = 1; rec[2].yMin = 0f; rec[2].yMax = 0.25f;
        mergeTxMgr.Instance.getBlcokBytes(textures[0], 1024);
        mergeTxMgr.Instance.getBlcokBytes(textures[1], 1024);
        int blockByte = mergeTxMgr.Instance.getBlcokBytes(textures[2], 1024);
        mergeTxMgr.Instance.getByteInTx(rec[0].xMin, rec[0].yMin, mergeTxMgr.Instance.data, blockByte, xyMax, textures[0]);
        mergeTxMgr.Instance.getByteInTx(rec[1].xMin, rec[1].yMin, mergeTxMgr.Instance.data, blockByte, xyMax, textures[1]);
        mergeTxMgr.Instance.getByteInTx(rec[2].xMin, rec[2].yMin, mergeTxMgr.Instance.data, blockByte, xyMax, textures[2]);
        ss.Dispose();
        var combinedTex = new Texture2D(xyMax, xyMax, textures[0].format, false);
        combinedTex.LoadRawTextureData(mergeTxMgr.Instance.data);
        combinedTex.Apply(false, true);


        Vector2[] atlasUVs = new Vector2[uvCount];
        //as combine textures into single texture,so need recalculate uvs			 
        int j = 0;
        for (int i = 0; i < uvList.Count; i++)
        {
            foreach (Vector2 uv in uvList[i])
            {
                atlasUVs[j].x = Mathf.Lerp(rec[i].xMin, rec[i].xMax, uv.x);
                atlasUVs[j].y = Mathf.Lerp(rec[i].yMin, rec[i].yMax, uv.y);
                int sq = i + 1;
                //Debug.Log("第" + sq + "个矩形" + "xMin == " + rec[i].xMin + "   xMax == " + rec[i].xMax+"  原始uvX =="+uv.x+
                //    "  合并后 == " + atlasUVs[j].x + "yMin == " + rec[i].yMin + "   yMax == " + rec[i].yMax + "  原始uvY ==" 
                //    + uv.y + "  合并后 == " + atlasUVs[j].y);
                j++;
            }
        }

        r.material.mainTexture = combinedTex;
        r.sharedMesh.uv = atlasUVs;

        return root;
    }
}