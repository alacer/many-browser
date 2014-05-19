/****************************************
	Simple Mesh Combine Editor v1.2				
	Copyright 2013 Unluck Software	
 	www.chemicalbliss.com
 	
 	Change Log
 		v1.1
 		Added naming and prefab save option	
 		v1.2
 		Added lightmap support																																		
*****************************************/
import System.IO;
@CustomEditor(SimpleMeshCombine)

public class SimpleMeshCombineEditor extends Editor {
	
    override function OnInspectorGUI () {
    	//DrawDefaultInspector ();
   		GUILayout.Space(10);
		GUILayout.Label("*All meshes must have same material*");	
		GUILayout.Space(10);
		if(!target.combined){
		target._generateLightmapUV = EditorGUILayout.Toggle("Generate Ligthmap UV's", target._generateLightmapUV);
		GUILayout.Label("Combine all Mesh Renderer enabled meshes");
			if(GUILayout.Button("Combine")) {
				if(target.transform.childCount > 1) combineMeshes();
			}   
       }else{
			GUILayout.Label("Decombine all previously combined meshes");
			if(GUILayout.Button("Release")) {
				EnableRenderers(true);
				target._savedPrefab = false;
					if(target.combined)
						DestroyImmediate(target.combined);
			}
        }
        if(target.combined && !target._savedPrefab)
        	target._advanced = EditorGUILayout.Toggle("Advanced Features", target._advanced);
        if(target.combined && target._advanced && !target._savedPrefab){
        	if(GUILayout.Button("Save Prefab")) {
        		var n:String = target.meshName;
        		if(System.IO.Directory.Exists("Assets/Simple Mesh Combine/Saved Meshes/")){
        		if(!System.IO.File.Exists("Assets/Simple Mesh Combine/Saved Meshes/"+target.meshName+".asset")){     	
        			AssetDatabase.CreateAsset(target.combined.GetComponent(MeshFilter).sharedMesh, "Assets/Simple Mesh Combine/Saved Meshes/"+n+".asset");
        			target._advanced = false;
        			target._savedPrefab = true;
        			Debug.Log("Saved Assets/Simple Mesh Combine/Saved Meshes/"+n+".asset");
        		}else{
        			Debug.Log(target.meshName+".asset" + " already exists, please change the name");
        		}
        		
        		}else{
        			Debug.Log("Missing Folder: Assets/Simple Mesh Combine/Saved Meshes/");
        		}
        	}
        	target.meshName = GUILayout.TextField(target.meshName);
        }
        if (GUI.changed){
                EditorUtility.SetDirty(target);
        }
   }
   
	function EnableRenderers(e:boolean) {	
    	for (var i:int = 0; i < target.combinedGameOjects.length; i++){
        	target.combinedGameOjects[i].renderer.enabled = e;
   		}  
	}
	
	function FindEnabledMeshes() 
	{//Returns a meshFilter[] list of all renderer enabled meshfilters(so that it does not merge disabled meshes, useful when there are invisible box colliders)
		var renderers;
		var count:int;
		renderers = target.transform.GetComponentsInChildren(MeshFilter);
			
		for (var i:int = 0; i < renderers.length; i++)
		{//count all the enabled meshrenderers in children
			if(renderers[i].GetComponent(MeshRenderer) && renderers[i].GetComponent(MeshRenderer).enabled)
				count++;
		}
		var meshfilters = new MeshFilter[count];//creates a new array with the correct length
		count = 0;
		for (var ii:int = 0; ii < renderers.length; ii++)
		{//adds all enabled meshes to the array
			if(renderers[ii].GetComponent(MeshRenderer) && renderers[ii].GetComponent(MeshRenderer).enabled){
				meshfilters[count] = renderers[ii];
				count++;
			}
		}
		return meshfilters;
	}
	
	function combineMeshes () 
	{//Combines meshes
		var combinedFrags:GameObject = new GameObject();
		combinedFrags.AddComponent(MeshFilter);
		combinedFrags.AddComponent(MeshRenderer);		
		var meshFilters;
		meshFilters = FindEnabledMeshes();
	    var combine: CombineInstance[] = new CombineInstance[meshFilters.length];
	      
	    Debug.Log("Simple Mesh Combine: Combined " + meshFilters.length + " Meshes");
	      
	    target.combinedGameOjects = new GameObject[meshFilters.length];      
	    for (var i:int = 0; i < meshFilters.length; i++)
	    {
	    	combinedFrags.GetComponent(MeshRenderer).sharedMaterial = meshFilters[i].transform.gameObject.GetComponent(MeshRenderer).sharedMaterial;
	    	target.combinedGameOjects[i] = meshFilters[i].gameObject;
	        combine[i].mesh = meshFilters[i].transform.GetComponent(MeshFilter).sharedMesh;
	        combine[i].transform = meshFilters[i].transform.localToWorldMatrix;  		
	    }
	    
	    combinedFrags.GetComponent(MeshFilter).mesh = new Mesh();
	    combinedFrags.GetComponent(MeshFilter).sharedMesh.CombineMeshes(combine);
	    if(target._generateLightmapUV){
	   		Unwrapping.GenerateSecondaryUVSet(combinedFrags.GetComponent(MeshFilter).sharedMesh);
	   		combinedFrags.isStatic = true;
	   	}
	   		
	    combinedFrags.name = "_Combined Mesh [" + target.name + "]";
	    target.combined = combinedFrags.gameObject;
	    EnableRenderers(false);
	    combinedFrags.transform.parent = target.transform;
    }	
}