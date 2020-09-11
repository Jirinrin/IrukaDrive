using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	//[CustomEditor( typeof( ShapeRenderer ) )]
	[CanEditMultipleObjects]
	public class ShapeRendererEditor : Editor {

		// ShapeRenderer
		protected SerializedProperty propColor;
		SerializedProperty propBlendMode = null;
		SerializedProperty propScaleMode = null;

		// MeshRenderer
		SerializedObject soRnd;
		SerializedProperty propSortingOrder;
		SerializedProperty propSortingLayer;

		static GUIContent blendModeGuiContent = new GUIContent(
			"Blend Mode",
			"Opaque does not support partial transparency, " +
			"but will write to the depth buffer and sort correctly. " +
			"For best results, use MSAA in your project to avoid aliasing " +
			"(note that it may still be aliased in the scene view)\n" +
			"\n" +
			"Transparent supports partial transparency, " +
			"but may not sort properly in some cases.\n" +
			"\n" +
			"Additive is good for glowing/brightening effects against dark backgrounds\n" +
			"\n" +
			"Multiplicative is good for tinting/darkening effects against bright backgrounds"
		);

		static GUIContent scaleModeGuiContent = new GUIContent(
			"Scale Mode",
			"Uniform mode means thickness will also scale with the transform, regardless of thickness space settings\n\n" +
			"Coordinate mode means thickness values will remain the same even when scaling"
		);

		public virtual void OnEnable() {

			soRnd = new SerializedObject( targets.Select( t => ( (Component)t ).GetComponent<MeshRenderer>() as Object ).ToArray() );
			propSortingOrder = soRnd.FindProperty( "m_SortingOrder" );
			propSortingLayer = soRnd.FindProperty( "m_SortingLayerID" );

			// will assign all null properties, even in derived types
			FindAllProperties();
			
			// hide mesh filter/renderer components
			foreach( ShapeRenderer shape in targets.Cast<ShapeRenderer>() )
				shape.HideMeshFilterRenderer();
		}

		void FindAllProperties() {
			IEnumerable<FieldInfo> GetFields( Type type ) {
				return type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic )
					.Where( x => x.FieldType == typeof(SerializedProperty) && x.Name.StartsWith( "m_" ) == false && x.GetValue( this ) == null );
			}

			IEnumerable<FieldInfo> fieldsBase = GetFields( GetType().BaseType );
			IEnumerable<FieldInfo> fieldsInherited = GetFields( GetType() );

			foreach( FieldInfo field in fieldsBase.Concat( fieldsInherited ) ) {
				string fieldName = char.ToLowerInvariant( field.Name[4] ) + field.Name.Substring( 5 );
				field.SetValue( this, serializedObject.FindProperty( fieldName ) );
				if( field.GetValue( this ) == null )
					Debug.LogError( $"Failed to load {target.GetType()} property: {field.Name} !=> {fieldName}" );
			}
		}

		protected void BeginProperties( bool showColor = true ) {
			soRnd.Update();
			using( ShapesUI.Horizontal ) {
				using( ShapesUI.TempFieldWidth( 180f ) )
					ShapesUI.RenderSortingLayerField( propSortingLayer );
				using( ShapesUI.TempLabelWidth( 40f ) )
					EditorGUILayout.PropertyField( propSortingOrder, new GUIContent( "Order" ) );
			}

			EditorGUILayout.PropertyField( propBlendMode, blendModeGuiContent );
			if( ( target as ShapeRenderer ).HasScaleModes )
				EditorGUILayout.PropertyField( propScaleMode, scaleModeGuiContent );
			if( showColor )
				PropertyFieldColor();
		}

		protected bool EndProperties() => soRnd.ApplyModifiedProperties() | serializedObject.ApplyModifiedProperties();

		protected void PropertyFieldColor() => EditorGUILayout.PropertyField( propColor );
		protected void PropertyFieldColor( string s ) => EditorGUILayout.PropertyField( propColor, new GUIContent( s ) );
		protected void PropertyFieldColor( GUIContent content ) => EditorGUILayout.PropertyField( propColor, content );

		public bool HasFrameBounds() => true;

		public Bounds OnGetFrameBounds() {
			if( serializedObject.isEditingMultipleObjects ) {
				// this only works for multiselecting shapes of the same type
				// todo: might be able to make a solution using Editor.CreateEditor shenanigans
				Bounds bounds = ( (ShapeRenderer)serializedObject.targetObjects[0] ).GetWorldBounds();
				for( int i = 1; i < serializedObject.targetObjects.Length; i++ )
					bounds.Encapsulate( ( (ShapeRenderer)serializedObject.targetObjects[i] ).GetWorldBounds() );
				return bounds;
			} else {
				return ( (ShapeRenderer)target ).GetWorldBounds();
			}
		}

	}

}