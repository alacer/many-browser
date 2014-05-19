/****************************************
	Simple Mesh Combine v1.2					
	Copyright 2013 Unluck Software	
 	www.chemicalbliss.com
 	
 	Change Log
 		v1.1
 		Added naming and prefab save option		
 		v1.2
 		Added lightmap support																																	
*****************************************/
//Add the script to the parent gameObject, then click combine

@script AddComponentMenu("Simple Mesh Combine")
#pragma strict
	var combinedGameOjects:GameObject[];	//Stores gameObjects that has been merged, mesh renderer disabled
	var combined:GameObject;				//Stores the combined mesh gameObject
	var meshName:String = "Combined_Meshes";//Asset name when saving as prefab
	var _advanced:boolean;					//Toggles advanced features
	var _savedPrefab:boolean;				//Used when checking if this mesh has been saved to prefab (saving the same mesh twice generates error)
	var _generateLightmapUV:boolean;		//Toggles secondary UV map generation