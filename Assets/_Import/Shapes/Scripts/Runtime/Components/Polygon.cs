using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Polygon" )]
	public class Polygon : ShapeRenderer {

		#if UNITY_EDITOR
		public List<Vector2> PolyPoints => polyPoints;
		#endif
		[SerializeField] List<Vector2> polyPoints = new List<Vector2>() {
			new Vector2( 1f, 0f ),
			new Vector2( 0.5f, 0.86602545f ),
			new Vector2( -0.5f, 0.8660254f ),
			new Vector2( -1f, 0f ),
			new Vector2( -0.5f, -0.86602545f ),
			new Vector2( 0.5f, -0.86602545f )
		};

		// also called alignment
		[SerializeField] PolygonTriangulation triangulation = PolygonTriangulation.EarClipping;
		public PolygonTriangulation Triangulation {
			get => triangulation;
			set {
				triangulation = value;
				meshOutOfDate = true;
			}
		}

		// global color fill gradient shenanigans
		#if UNITY_EDITOR
		public ShapeFill Fill => fill;
		#endif
		[SerializeField] ShapeFill fill = new ShapeFill();
		[SerializeField] bool useFill = false;
		int FillTypeShaderInt => useFill ? fill.GetShaderFillModeInt() : ShapeFill.FILL_NONE;
		public bool UseFill {
			get => useFill;
			set {
				useFill = value;
				SetIntNow( ShapesMaterialUtils.propFillType, FillTypeShaderInt );
			}
		}
		public FillType FillType {
			get => fill.type;
			set {
				fill.type = value;
				SetIntNow( ShapesMaterialUtils.propFillType, FillTypeShaderInt );
			}
		}
		public FillSpace FillSpace {
			get => fill.space;
			set => SetIntNow( ShapesMaterialUtils.propFillSpace, (int)( fill.space = value ) );
		}
		public Vector3 FillRadialOrigin {
			get => fill.radialOrigin;
			set {
				fill.radialOrigin = value;
				SetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
			}
		}
		public float FillRadialRadius {
			get => fill.radialRadius;
			set {
				fill.radialRadius = value;
				SetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
			}
		}
		public Vector3 FillLinearStart {
			get => fill.linearStart;
			set {
				fill.linearStart = value;
				SetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
			}
		}
		public Vector3 FillLinearEnd {
			get => fill.linearEnd;
			set => SetVector3Now( ShapesMaterialUtils.propFillEnd, fill.linearEnd = value );
		}
		public Color FillColorStart {
			get => fill.colorStart;
			set => SetColorNow( ShapesMaterialUtils.propColor, fill.colorStart = value );
		}
		public Color FillColorEnd {
			get => fill.colorEnd;
			set => SetColorNow( ShapesMaterialUtils.propColorEnd, fill.colorEnd = value );
		}


		public int Count => polyPoints.Count;
		public Vector2 this[ int i ] {
			get => polyPoints[i];
			set {
				polyPoints[i] = value;
				meshOutOfDate = true;
			}
		}

		public void SetPointPosition( int index, Vector2 position ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			polyPoints[index] = position;
			meshOutOfDate = true;
		}

		public void SetPoints( IEnumerable<Vector2> points ) {
			this.polyPoints.Clear();
			AddPoints( points );
		}

		public void AddPoints( IEnumerable<Vector2> points ) {
			polyPoints.AddRange( points );
			meshOutOfDate = true;
		}

		public void AddPoint( Vector2 point ) {
			polyPoints.Add( point );
			meshOutOfDate = true;
		}

		// todo: move this to base class?
		bool meshOutOfDate = true;
		protected override bool UseCamOnPreCull => true;

		protected override void CamOnPreCull() {
			if( meshOutOfDate ) {
				meshOutOfDate = false;
				UpdateMesh( force: true );
			}
		}

		protected override void SetAllMaterialProperties() {
			// only uses base properties
			if( useFill ) {
				SetInt( ShapesMaterialUtils.propFillSpace, (int)fill.space );
				SetVector4( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
				SetVector3( ShapesMaterialUtils.propFillEnd, fill.linearEnd );
				SetColor( ShapesMaterialUtils.propColor, fill.colorStart );
				SetColor( ShapesMaterialUtils.propColorEnd, fill.colorEnd );
			}

			SetInt( ShapesMaterialUtils.propFillType, FillTypeShaderInt );
		}

		public override bool HasScaleModes => false;
		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matPolygon[BlendMode] };
		protected override MeshUpdateMode MeshUpdateMode => MeshUpdateMode.SelfGenerated;

		protected override void GenerateMesh() => ShapesMeshGen.GenPolygonMesh( Mesh, polyPoints, triangulation );

		protected override Bounds GetBounds() {
			if( polyPoints.Count < 2 )
				return default;
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;
			foreach( Vector3 pt in polyPoints ) {
				min = Vector3.Min( min, pt );
				max = Vector3.Max( max, pt );
			}

			return new Bounds( ( max + min ) * 0.5f, max - min );
		}

	}

}