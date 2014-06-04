using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	// class ComponentEx
	
	public class ComponentEx
	{
		private Component com;
		
		public ComponentEx()
		{
		}
		public ComponentEx(GameObject go)
		{
			this.Set(go);
		}
		public ComponentEx(Component com)
		{
			this.Set(com);
		}
		
		public void Set(GameObject go)
		{
			this.Set(go.transform);
		}
		public void Set(Component com)
		{
			if (this.com != com)
			{
				this.com = com;
				this.animation = com.animation;
				this.camera = com.camera;
				this.collider = com.collider;
				this.constantForce = com.constantForce;
				this.gameObject = com.gameObject;
				this.hingeJoint = com.hingeJoint;
				this.light = com.light;
				this.networkView = com.networkView;
				this.particleEmitter = com.particleEmitter;
				this.particleSystem = com.particleSystem;
				this.renderer = com.renderer;
				this.rigidbody = com.rigidbody;
				this.transform = com.transform;
				this.meshFilter = com.gameObject.GetComponent<MeshFilter>();
			}
		}
		
		public virtual Animation animation
		{
			get; internal set;
		}
		public virtual Camera camera
		{
			get; internal set;
		}
		public virtual Collider collider
		{
			get; internal set;
		}
		public virtual ConstantForce constantForce
		{
			get; internal set;
		}
		public virtual GameObject gameObject
		{
			get; internal set;
		}
		public virtual HingeJoint hingeJoint
		{
			get; internal set;
		}
		public virtual Light light
		{
			get; internal set;
		}
		public virtual NetworkView networkView
		{
			get; internal set;
		}
		public virtual ParticleEmitter particleEmitter
		{
			get; internal set;
		}
		public virtual ParticleSystem particleSystem
		{
			get; internal set;
		}
		public virtual Renderer renderer
		{
			get; internal set;
		}
		public virtual Rigidbody rigidbody
		{
			get; internal set;
		}
		public virtual Transform transform
		{
			get; internal set;
		}
		
		public virtual MeshFilter meshFilter
		{
			get; internal set;
		}
		public virtual Mesh mesh
		{
			get { return this.meshFilter.sharedMesh; }
		}
		
		public T GetComponent<T>() where T : Component
		{
			return this.gameObject.GetComponent<T>();
		}
		public T GetComponentInChildren<T>() where T : Component
		{
			return this.gameObject.GetComponentInChildren<T>();
		}
		public T[] GetComponentsInChildren<T>() where T : Component
		{
			return this.gameObject.GetComponentsInChildren<T>();
		}
	};
	
	// class MonoBehaviourEx
	
	public class MonoBehaviourEx : MonoBehaviour
	{
		private ComponentEx cache = new ComponentEx();
		
		public new Animation animation
		{
			get { this.cache.Set(this); return this.cache.animation; }
		}
		public new Camera camera
		{
			get { this.cache.Set(this); return this.cache.camera; }
		}
		public new Collider collider
		{
			get { this.cache.Set(this); return this.cache.collider; }
		}
		public new ConstantForce constantForce
		{
			get { this.cache.Set(this); return this.cache.constantForce; }
		}
		public new GameObject gameObject
		{
			get { this.cache.Set(this); return this.cache.gameObject; }
		}
		public new HingeJoint hingeJoint
		{
			get { this.cache.Set(this); return this.cache.hingeJoint; }
		}
		public new Light light
		{
			get { this.cache.Set(this); return this.cache.light; }
		}
		public new NetworkView networkView
		{
			get { this.cache.Set(this); return this.cache.networkView; }
		}
		public new ParticleEmitter particleEmitter
		{
			get { this.cache.Set(this); return this.cache.particleEmitter; }
		}
		public new Renderer renderer
		{
			get { this.cache.Set(this); return this.cache.renderer; }
		}
		public new Rigidbody rigidbody
		{
			get { this.cache.Set(this); return this.cache.rigidbody; }
		}
		public new Transform transform
		{
			get { this.cache.Set(this); return this.cache.transform; }
		}
		
		public MeshFilter meshFilter
		{
			get { this.cache.Set(this); return this.cache.meshFilter; }
		}
		public Mesh mesh
		{
			get { this.cache.Set(this); return this.cache.mesh; }
		}
	};
}