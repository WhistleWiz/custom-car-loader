﻿using System;
using System.Collections.Generic;
using System.Linq;
using CCL_GameScripts.Attributes;
using UnityEngine;

namespace CCL_GameScripts.CabControls
{
    public class LeverSetup : ControlSetupBase, IApertureTrackable
    {
        public override string TargetTypeName => "DV.CabControls.Spec.Lever";
        public override bool DestroyAfterCreation => false;
        public override CabControlType ControlType => CabControlType.Lever;

		[Header("Lever")]
		[ProxyField("useSteppedJoint")]
		public bool UseNotches = true; // useSteppedJoint
		[ProxyField("notches")]
		public int Notches = 20;
		[ProxyField("invertDirection")]
		public bool InvertDirection = false;
		
		[Header("Door")]
		public bool TrackAsDoor = false;
		public bool TrackAsAperture => TrackAsDoor;

		[SerializeField]
		private int _cabNumber = -1;
		public int CabNumber
		{
			get => _cabNumber;
			set => _cabNumber = value;
		}

		[Header("Hinge Joint")]
		[ProxyField("jointAxis")]
		public Vector3 JointAxis = Vector3.up;
		[ProxyField]
		public bool useLimits = true;
		[ProxyField("jointLimitMin")]
		public float JointLimitMin = 0;
		[ProxyField("jointLimitMax")]
		public float JointLimitMax = 90;
		[ProxyField]
		public bool useSpring = true;
		[ProxyField("jointSpring")]
		public float JointSpring = 100f;
		[ProxyField("jointDamper")]
		public float JointDamper = 10;

		[Header("Rigidbody")]
		[ProxyField("rigidbodyMass")]
		public float RigidbodyMass = 30;
		[ProxyField("rigidbodyDrag")]
		public float RigidbodyDrag = 8;
		//public float RigidbodyAngularDrag = 0;

		[Header("NonVR")]
		[ProxyField("scrollWheelHoverScroll")]
		public float HoverScrollMagnitude = 4;
		[ProxyField("scrollWheelSpring")]
		public float ScrollWheelSpring = 50;
		[ProxyComponent("nonVrStaticInteractionArea", "StaticInteractionArea")]
		public GameObject StaticInteractionArea = null;

		[Header("VR")]
		[ProxyField("maxForceAppliedMagnitude")]
		public float MaxAppliedForce = float.PositiveInfinity;
		[ProxyField("pullingForceMultiplier")]
		public float PullingForceMultiplier = 1;

		[InspectorName("Interaction Point (optional)")]
		[ProxyField("interactionPoint")]
		public Transform InteractionPoint = null;
		[ProxyField("limitVibration")]
		public bool VibrateAtLimit = false;

		// TODO: Audio

		protected const float GIZMO_RADIUS = 0.1f;
		protected const int GIZMO_SEGMENTS = 40;
		protected static readonly Color START_COLOR = new Color(0.65f, 0, 0);
		protected static readonly Color END_COLOR = new Color(0, 0.65f, 0);

		private void OnDrawGizmos()
        {
			Color startColor = InvertDirection ? END_COLOR : START_COLOR;
			Color endColor = InvertDirection ? START_COLOR : END_COLOR;

			Vector3 massCenter = ModelUtil.GetModelCenterline(gameObject);
			Vector3 massZeroVector = transform.InverseTransformPoint(massCenter);
			Vector3 gizmoZeroVector = Vector3.ProjectOnPlane(massZeroVector, JointAxis);

			float massLength = massZeroVector.magnitude;
			Vector3 massPivot = Vector3.Project(massZeroVector, JointAxis);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, transform.TransformPoint(massPivot));

			if( UseNotches && (Notches > 0) )
			{
				float rayCount = Notches - 1;

				// draw ray segments
				for( int i = 0; i <= rayCount; i++ )
				{
					Gizmos.color = Color.Lerp(startColor, endColor, i / rayCount);

					float rotateAngle = Mathf.Lerp(JointLimitMin, JointLimitMax, i / rayCount);
					Quaternion rotation = Quaternion.AngleAxis(rotateAngle, JointAxis);

					// projected sweep
                    Vector3 rayVector = transform.TransformPoint((rotation * gizmoZeroVector) * GIZMO_RADIUS);
					Gizmos.DrawLine(transform.position, rayVector);

					// center of mass sweep
					rayVector = transform.TransformPoint(rotation * massZeroVector);
					Gizmos.DrawLine(transform.TransformPoint(massPivot), rayVector);
				}
			}
			else
            {
				// draw semi-circle
				Vector3 lastVector = transform.position;
				Vector3 lastMassVector = transform.TransformPoint(massPivot);

				for( int i = 0; i <= GIZMO_SEGMENTS; i++ )
				{
					Gizmos.color = Color.Lerp(startColor, endColor, (float)i / GIZMO_SEGMENTS);

					float rotateAngle = Mathf.Lerp(JointLimitMin, JointLimitMax, (float)i / GIZMO_SEGMENTS);
					Quaternion rotation = Quaternion.AngleAxis(rotateAngle, JointAxis);

					// projected sweep
					Vector3 nextVector = transform.TransformPoint((rotation * gizmoZeroVector) * GIZMO_RADIUS);
					Vector3 nextMassVector = transform.TransformPoint(rotation * massZeroVector);

					if( i == 0 || i == GIZMO_SEGMENTS )
                    {
						Gizmos.DrawLine(transform.position, nextVector);
						Gizmos.DrawLine(transform.TransformPoint(massPivot), nextMassVector);
					}
					if( i != 0 )
                    {
						Gizmos.DrawLine(lastVector, nextVector);
						Gizmos.DrawLine(lastMassVector, nextMassVector);
					}

					lastVector = nextVector;
					lastMassVector = nextMassVector;
				}
			}
        }

		[MethodButton(
			nameof(IndependentBrakeDefaults),
			nameof(TrainBrakeDefaults),
			nameof(DieselThrottleDefaults),
			nameof(DieselReverserDefaults))]
		[SerializeField]
		private bool editorFoldout;

		public void IndependentBrakeDefaults()
		{
			UseNotches = true;
			Notches = 20;
			InvertDirection = false;

			JointAxis = Vector3.up;
			JointLimitMin = -61;
			JointLimitMax = 0;
			JointSpring = 100f;
			JointDamper = 10;

			RigidbodyMass = 30;
			RigidbodyDrag = 8;

			HoverScrollMagnitude = 4;
			ScrollWheelSpring = 50;

			MaxAppliedForce = float.PositiveInfinity;
			PullingForceMultiplier = 1;
			VibrateAtLimit = false;
		}

		public void TrainBrakeDefaults()
        {
			UseNotches = true;
			Notches = 20;
			InvertDirection = false;

			JointAxis = Vector3.up;
			JointLimitMin = 0;
			JointLimitMax = 72;
			JointSpring = 100f;
			JointDamper = 10;

			RigidbodyMass = 30;
			RigidbodyDrag = 8;

			HoverScrollMagnitude = 4;
			ScrollWheelSpring = 50;

			MaxAppliedForce = float.PositiveInfinity;
			PullingForceMultiplier = 1;
			VibrateAtLimit = false;
		}

		public void DieselThrottleDefaults()
        {
			UseNotches = true;
			Notches = 8;
			InvertDirection = false;

			JointAxis = Vector3.up;
			JointLimitMin = -52;
			JointLimitMax = 0;
			JointSpring = 100f;
			JointDamper = 0;

			RigidbodyMass = 30;
			RigidbodyDrag = 16;

			HoverScrollMagnitude = 1;
			ScrollWheelSpring = 100;

			MaxAppliedForce = float.PositiveInfinity;
			PullingForceMultiplier = 1;
			VibrateAtLimit = false;
		}

		public void DieselReverserDefaults()
        {
			UseNotches = true;
			Notches = 3;
			InvertDirection = false;

			JointAxis = Vector3.up;
			JointLimitMin = -25;
			JointLimitMax = 65;
			JointSpring = 25;
			JointDamper = 2;

			RigidbodyMass = 30;
			RigidbodyDrag = 0;

			HoverScrollMagnitude = 1;
			ScrollWheelSpring = 8;

			MaxAppliedForce = float.PositiveInfinity;
			PullingForceMultiplier = 1;
			VibrateAtLimit = false;
		}
	}
}
