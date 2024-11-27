﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.ViconUnityStream
{
    public class CustomHandScriptV2 : CustomSubjectScript
    {
        /// <summary>
        /// The <see cref="CustomHandScript"/> objects for each hands currently active in the scene. Will be used by the hand subsystem provider.
        /// </summary>
        internal static Dictionary<Handedness, CustomHandScriptV2> activeHandScripts = new Dictionary<Handedness, CustomHandScriptV2>();

        public float normalOffset = 0.001f;
        public bool setPosition = true;
        public bool setScale = true;
        public float scaleToSet = 0.02f;
        private Vector3 normal;
        private Vector3 palm;
        private bool noHand = false;
        [SerializeField]
        public Handedness handedness = Handedness.Right;

        // NOTE: Considered using an enum with a dictionary, but that
        // means using a dictionary lookup everytime a name is needed,
        // so doing it the ugly (or not?) way
        protected string segment_Arm = "Arm";
        protected string segment_Hand = "Hand";
        protected string segment_1D1 = "1D1";
        protected string segment_1D2 = "1D2";
        protected string segment_1D3 = "1D3";
        protected string segment_1D4 = "1D4";
        protected string segment_2D1 = "2D1";
        protected string segment_2D2 = "2D2";
        protected string segment_2D3 = "2D3";
        protected string segment_2D4 = "2D4";
        protected string segment_3D1 = "3D1";
        protected string segment_3D2 = "3D2";
        protected string segment_3D3 = "3D3";
        protected string segment_3D4 = "3D4";
        protected string segment_4D1 = "4D1";
        protected string segment_4D2 = "4D2";
        protected string segment_4D3 = "4D3";
        protected string segment_4D4 = "4D4";
        protected string segment_5D1 = "5D1";
        protected string segment_5D2 = "5D2";
        protected string segment_5D3 = "5D3";
        protected string segment_5D4 = "5D4";

        // Hand and arm markers
        protected string marker_FA2 = "FA2";
        protected string marker_FA1 = "FA1";
        protected string marker_WRA = "WRA";
        protected string marker_WRB = "WRB";

        // thumb markers
        protected string marker_TH1 = "TH1";
        protected string marker_TH2 = "TH2";
        protected string marker_TH3 = "TH3";
        protected string marker_TH3P = "TH3P";
        protected string marker_TH4 = "TH4";

        // index finger markers
        protected string marker_H2 = "H2";
        protected string marker_IF1 = "IF1";
        protected string marker_IF2 = "IF2";
        protected string marker_IF3 = "IF3";

        // middle finger markers
        protected string marker_H3 = "H3";
        protected string marker_TF1 = "TF1";
        protected string marker_TF2 = "TF2";
        protected string marker_TF3 = "TF3";

        // ring finger markers
        protected string marker_H4 = "H4";
        protected string marker_RF2 = "RF2";
        protected string marker_RF3 = "RF3";
        protected string marker_RF4 = "RF4";

        // pinky finger markers
        protected string marker_H5 = "H5";
        protected string marker_PF1 = "PF1";
        protected string marker_PF2 = "PF2";
        protected string marker_PF3 = "PF3";

        protected string finger_1 = "1";
        protected string finger_2 = "2";
        protected string finger_3 = "3";
        protected string finger_4 = "4";
        protected string finger_5 = "5";

        protected Dictionary<string, Vector3> baseVectors = new Dictionary<string, Vector3>();
        protected Dictionary<string, Vector3> previousSegments = new Dictionary<string, Vector3>();

        protected Dictionary<string, string> segmentChild;
        protected Dictionary<string, string> segmentParents;
        protected Dictionary<string, List<string>> fingerSegments;

        protected Dictionary<string, XRHandJointID> segmentToJointMapping;
        protected Dictionary<XRHandJointID, Pose> xrJointPoses;

        protected override void Start()
        {
            base.Start();
            string prefix = handedness == Handedness.Right ? "R": "L";

            segment_1D1 = prefix + segment_1D1;
            segment_1D2 = prefix + segment_1D2;
            segment_1D3 = prefix + segment_1D3;
            segment_1D4 = prefix + segment_1D4;
            segment_2D1 = prefix + segment_2D1;
            segment_2D2 = prefix + segment_2D2;
            segment_2D3 = prefix + segment_2D3;
            segment_2D4 = prefix + segment_2D4;
            segment_3D1 = prefix + segment_3D1;
            segment_3D2 = prefix + segment_3D2;
            segment_3D3 = prefix + segment_3D3;
            segment_3D4 = prefix + segment_3D4;
            segment_4D1 = prefix + segment_4D1;
            segment_4D2 = prefix + segment_4D2;
            segment_4D3 = prefix + segment_4D3;
            segment_4D4 = prefix + segment_4D4;
            segment_5D1 = prefix + segment_5D1;
            segment_5D2 = prefix + segment_5D2;
            segment_5D3 = prefix + segment_5D3;
            segment_5D4 = prefix + segment_5D4;
            marker_FA2 = prefix + marker_FA2;
            marker_FA1 = prefix + marker_FA1;
            marker_WRA = prefix + marker_WRA;
            marker_WRB = prefix + marker_WRB;
            marker_TH1 = prefix + marker_TH1;
            marker_TH2 = prefix + marker_TH2;
            marker_TH3 = prefix + marker_TH3;
            marker_TH3P = prefix + marker_TH3P;
            marker_TH4 = prefix + marker_TH4;
            marker_H2 = prefix + marker_H2;
            marker_IF1 = prefix + marker_IF1;
            marker_IF2 = prefix + marker_IF2;
            marker_IF3 = prefix + marker_IF3;
            marker_H3 = prefix + marker_H3;
            marker_TF1 = prefix + marker_TF1;
            marker_TF2 = prefix + marker_TF2;
            marker_TF3 = prefix + marker_TF3;
            marker_H4 = prefix + marker_H4;
            marker_RF2 = prefix + marker_RF2;
            marker_RF3 = prefix + marker_RF3;
            marker_RF4 = prefix + marker_RF4;
            marker_H5 = prefix + marker_H5;
            marker_PF1 = prefix + marker_PF1;
            marker_PF2 = prefix + marker_PF2;
            marker_PF3 = prefix + marker_PF3;
            finger_1 = prefix + finger_1;
            finger_2 = prefix + finger_2;
            finger_3 = prefix + finger_3;
            finger_4 = prefix + finger_4;
            finger_5 = prefix + finger_5;

            segmentChild = new Dictionary<string, string>()
            {
                //{segment_Arm, null},
                {segment_Arm, segment_Hand},
                {segment_Hand, segment_3D1},

                {segment_1D1, segment_1D2},
                {segment_1D2, segment_1D3},
                {segment_1D3, segment_1D4},

                {segment_2D1, segment_2D2},
                {segment_2D2, segment_2D3},
                {segment_2D3, segment_2D4},

                {segment_3D1, segment_3D2},
                {segment_3D2, segment_3D3},
                {segment_3D3, segment_3D4},

                {segment_4D1, segment_4D2},
                {segment_4D2, segment_4D3},
                {segment_4D3, segment_4D4},

                {segment_5D1, segment_5D2},
                {segment_5D2, segment_5D3},
                {segment_5D3, segment_5D4},
            };

            segmentParents = new Dictionary<string, string>()
            {
                //{segment_Arm, null},
                {segment_Hand, segment_Arm},
                //{segment_1D1, segment_Hand},
                //{segment_2D1, segment_Hand},
                {segment_3D1, segment_Hand},
                //{segment_4D1, segment_Hand},
                //{segment_5D1, segment_Hand},

                {segment_1D2, segment_1D1},
                {segment_2D2, segment_2D1},
                {segment_3D2, segment_3D1},
                {segment_4D2, segment_4D1},
                {segment_5D2, segment_5D1},

                {segment_1D3, segment_1D2},
                {segment_2D3, segment_2D2},
                {segment_3D3, segment_3D2},
                {segment_4D3, segment_4D2},
                {segment_5D3, segment_5D2},

                {segment_1D4, segment_1D3},
                {segment_2D4, segment_2D3},
                {segment_3D4, segment_3D3},
                {segment_4D4, segment_4D3},
                {segment_5D4, segment_5D3},
            };

            segmentMarkers = new Dictionary<string, List<string>>() {
                { segment_Arm, new List<string>() {marker_FA2, marker_FA1}} ,//{ marker_WRB, marker_FA2, marker_FA1, marker_WRA } } ,
                {    segment_Hand, new List<string>() { marker_WRB, marker_WRA }} ,//{"RH1", marker_H3, "RH6"}},
                {    segment_1D1,  new List<string>(){marker_TH1}},
                {    segment_1D2, new List<string>(){marker_TH2}},
                {    segment_1D3, new List<string>(){marker_TH3, marker_TH3P}},
                {    segment_1D4, new List<string>{marker_TH4}},

                {    segment_2D1, new List<string>{marker_H2}},
                {    segment_2D2, new List<string>{marker_IF1}},
                {    segment_2D3, new List<string>{marker_IF2}},
                {    segment_2D4, new List<string>{marker_IF3}},

                {    segment_3D1, new List<string>{marker_H3}},
                {    segment_3D2, new List<string>{marker_TF1}},
                {    segment_3D3, new List<string>{marker_TF2}},
                {    segment_3D4, new List<string>{marker_TF3}},

                {    segment_4D1, new List<string>{marker_H4}},
                {    segment_4D2, new List<string>{marker_RF2}},
                {    segment_4D3, new List<string>{marker_RF3}},
                {    segment_4D4, new List<string>{marker_RF4}},

                {   segment_5D1, new List<string>(){marker_H5}},
                {    segment_5D2,  new List<string>(){marker_PF1}},
                {    segment_5D3, new List<string>(){marker_PF2}},
                {    segment_5D4, new List<string>(){marker_PF3}}
            };

            fingerSegments = new Dictionary<string, List<string>>()
            {
                {finger_1, new List<string>{segment_1D1, segment_1D2, segment_1D3, segment_1D4}},
                {finger_2, new List<string>{segment_2D1, segment_2D2, segment_2D3, segment_2D4}},
                {finger_3, new List<string>{segment_3D1, segment_3D2, segment_3D3, segment_3D4}},
                {finger_4, new List<string>{segment_4D1, segment_4D2, segment_4D3, segment_4D4}},
                {finger_5, new List<string>{segment_5D1, segment_5D2, segment_5D3, segment_5D4}},
            };

            segmentToJointMapping = new Dictionary<string, XRHandJointID>()
            {
                {segment_Hand, XRHandJointID.Wrist},
                // {segment_Hand, segment_3D1},

                {segment_1D1, XRHandJointID.ThumbMetacarpal},
                {segment_1D2, XRHandJointID.ThumbProximal},
                {segment_1D3, XRHandJointID.ThumbDistal},
                {segment_1D4, XRHandJointID.ThumbTip},

                {segment_2D1, XRHandJointID.IndexProximal},
                {segment_2D2, XRHandJointID.IndexIntermediate},
                {segment_2D3, XRHandJointID.IndexDistal},
                {segment_2D4, XRHandJointID.IndexTip},

                {segment_3D1, XRHandJointID.MiddleProximal},
                {segment_3D2, XRHandJointID.MiddleIntermediate},
                {segment_3D3, XRHandJointID.MiddleDistal},
                {segment_3D4, XRHandJointID.MiddleTip},

                {segment_4D1, XRHandJointID.RingProximal},
                {segment_4D2, XRHandJointID.RingIntermediate},
                {segment_4D3, XRHandJointID.RingDistal},
                {segment_4D4, XRHandJointID.RingTip},

                {segment_5D1, XRHandJointID.LittleProximal},
                {segment_5D2, XRHandJointID.LittleIntermediate},
                {segment_5D3, XRHandJointID.LittleDistal},
                {segment_5D4, XRHandJointID.LittleTip},

            };
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            if (activeHandScripts.ContainsKey(handedness))
            {
                if (activeHandScripts[handedness] != this)
                {
                    Debug.LogWarning($"A CustomHandScript has already been registered for the {handedness}. Disabling thy self.");
                    gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning($"CustomSubjectScript in {transform.name} already registered as an active hand.");
                }
            }
            else
            {
                activeHandScripts.Add(handedness, this);
            }
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            if (activeHandScripts.ContainsKey(handedness) && activeHandScripts[handedness] == this)
            {
                activeHandScripts.Remove(handedness);
            }
        }

        protected bool isRightHand()
        {
            return handedness == Handedness.Right;
        }

        protected override Dictionary<string, Vector3> ProcessSegments(Dictionary<string, Vector3> segments, ViconStreamData viconStreamData)
        {
            /// Filling any gaps that can be filled
            if (gapFillingStrategy == GapFillingStrategy.FillRelative)
            {
                foreach(string segment in segments.Keys.ToList())
                {
                    if (segments[segment] == Vector3.zero)
                    {
                        segments[segment] = FillWithRelativeAdjacent(segment, segments);
                    }
                }
            }

            // What sorcery is this?
            palm = segments[segment_Hand] - (segments[segment_3D1] + 0.5f * (segments[segment_4D1] - segments[segment_4D1]));
            normal = Vector3.Cross(palm, segments[segment_4D1] - segments[segment_3D1]);
            if (!isRightHand())
            {
                 normal = -normal;
            }

            if (segmentChild.ContainsKey(segment_1D1) && segments[segmentChild[segment_1D1]] != Vector3.zero && segments[segment_1D1] != Vector3.zero)
            {
                baseVectors[finger_1] = segments[segmentChild[segment_1D1]] - segments[segment_1D1];
            }

            if (segmentChild.ContainsKey(segment_2D1) && segments[segmentChild[segment_2D1]] != Vector3.zero && segments[segment_2D1] != Vector3.zero)
            {
                baseVectors[finger_2] = segments[segmentChild[segment_2D1]] - segments[segment_2D1];
            }

            if (segmentChild.ContainsKey(segment_3D1) && segments[segmentChild[segment_3D1]] != Vector3.zero && segments[segment_3D1] != Vector3.zero)
            {
                baseVectors[finger_3] = segments[segmentChild[segment_3D1]] - segments[segment_3D1];
            }

            if (segmentChild.ContainsKey(segment_4D1) && segments[segmentChild[segment_4D1]] != Vector3.zero && segments[segment_4D1] != Vector3.zero)
            {
                baseVectors[finger_4] = segments[segmentChild[segment_4D1]] - segments[segment_4D1];
            }

            if (segmentChild.ContainsKey(segment_5D1) && segments[segmentChild[segment_5D1]] != Vector3.zero && segments[segment_5D1] != Vector3.zero)
            {
                baseVectors[finger_5] = segments[segmentChild[segment_5D1]] - segments[segment_5D1];
            }

            // Debug.Log(data.data[marker_TH3P] + "  -  "+ data.data[marker_TH3]);
            if (viconStreamData.data.ContainsKey(marker_TH3P) && viconStreamData.data.ContainsKey(marker_TH3))
            {
                var p1 = viconStreamData.data[marker_TH3P];
                var p2 = viconStreamData.data[marker_TH3];
                Vector3 p1Position = new Vector3(p1[0], p1[2], p1[1]);
                Vector3 p2Position = new Vector3(p2[0], p2[2], p2[1]);

                /// If one of the datapoints is missing, use the previous values, do this by not modifying the baseVectors
                if (p1[0] != 0 || p2[0] != 0)
                {
                    /// Ensure p1 and p2 are not switched, as it can happen when accuracy is low
                    /// PalmBase's Transform.up should be pointing into the palm.
                    /// This is based on the assumption that the RTH3P->RTH3 vector, relative to
                    /// PlamBase.up will be negative (pointing up from palm)

                    Vector3 p1p2Vector = p2Position - p1Position;
                    float dotPrduct = Vector3.Dot(p1p2Vector, isRightHand() ? normal : -normal);

                    if (dotPrduct < 0)
                    {
                        p1p2Vector = -p1p2Vector;
                    }

                    baseVectors["R1_right"] = p1p2Vector;
                }
            }

            /// Using segments to store the normal vector instead of position?
            segments["PalmBase"] = normal;// *0.01f;
            if (normal.magnitude > 0 && palm.magnitude > 0)
            {
                // Setting here so that the rootsegment gets the correct rotation.
                segmentsRotation[segment_Arm] = Quaternion.LookRotation(-normal, -palm);
            }

            if (palm == Vector3.zero)
                noHand = true;
            else
            {
                noHand = false;
                if (normal.magnitude > 0 && palm.magnitude > 0)
                {
                    transform.rotation = Quaternion.LookRotation(-normal, -palm);
                }
            }

            if (xrJointPoses == null)
            {
                xrJointPoses = new Dictionary<XRHandJointID, Pose>();
            }
            xrJointPoses.Clear();

            foreach(string segmentName in segmentMarkers.Keys)
            {
                Vector3 segmentPosition = segments[segmentName];
                Quaternion segmentRotation = segmentsRotation[segmentName];

                Debug.Log($"--{segmentName} {segmentPosition}");
                /// Ignore setting pos/rot/scale if GapFillingStrategy.Ignore and segmentPosition is zero
                /// and cannot resolve with adjacent segments (in FillRelative mode)
                if (!(gapFillingStrategy == GapFillingStrategy.Ignore || gapFillingStrategy == GapFillingStrategy.FillRelative) || segmentPosition != Vector3.zero)
                {
                    if (segmentName != "PalmBase")
                    {
                        string fingerId = segmentName.Substring(0, 2);

                        if (segmentChild.TryGetValue(segmentName, out var value))
                        {
                            Vector3 childSegment = segments[value];

                            /// Avoid setting rotation if childsegment is zero
                            if (childSegment != Vector3.zero && baseVectors.ContainsKey(fingerId))
                            {
                                Vector3 upDirection = childSegment - segmentPosition;
                                if (upDirection != Vector3.zero)
                                {
                                    Vector3 right;
                                    Vector3 forward;
                                    if (fingerId == finger_1)
                                    {
                                        right = baseVectors["R1_right"];
                                        //right = Vector3.Cross(normal, baseVectors[fingerId]);
                                        forward = Vector3.Cross(upDirection, right);
                                    }
                                    else
                                    {
                                        right = Vector3.Cross(normal, baseVectors[fingerId]);
                                        forward = Vector3.Cross(upDirection, right);
                                    }
                                    if (forward != Vector3.zero)
                                        segmentRotation = Quaternion.LookRotation(forward, upDirection);
                                }
                            }
                        }

                        Vector3 segmentForward = segmentRotation * Vector3.forward;

                        segments[segmentName] = segmentPosition + fingerId[1] switch
                            {
                                '1' => segmentForward * normalOffset * 0.9f,
                                '3' => segmentForward * normalOffset * 1.08f,
                                '4' => segmentForward * normalOffset * 1.13f,
                                '5' => segmentForward * normalOffset * 1.2f,
                                _ => segmentForward * normalOffset
                            };

                        segmentsRotation[segmentName] = segmentRotation;

                        if (segmentToJointMapping.ContainsKey(segmentName))
                        {
                            xrJointPoses.Add(segmentToJointMapping[segmentName], new Pose(segmentPosition, segmentRotation));
                        }
                    }
                }
                previousSegments[segmentName] = segmentPosition;
            }

            // FIXME: pass radius
            ViconXRLoader.TrySetHandSbsystemData(handedness, xrJointPoses, null);

            return segments;
        }

        private Matrix4x4 handWorldToLocalMatrix;

        protected override string ConstructFinalWriterString()
        {
            return "[" + base.ConstructFinalWriterString() + ", [" +
                handWorldToLocalMatrix[0, 0] + ", " + handWorldToLocalMatrix[0, 1] + ", " + handWorldToLocalMatrix[0, 2] + ", " + handWorldToLocalMatrix[0, 3] + ", " +
                handWorldToLocalMatrix[1, 0] + ", " + handWorldToLocalMatrix[1, 1] + ", " + handWorldToLocalMatrix[1, 2] + ", " + handWorldToLocalMatrix[1, 3] + ", " +
                handWorldToLocalMatrix[2, 0] + ", " + handWorldToLocalMatrix[2, 1] + ", " + handWorldToLocalMatrix[2, 2] + ", " + handWorldToLocalMatrix[2, 3] + ", " +
                handWorldToLocalMatrix[3, 0] + ", " + handWorldToLocalMatrix[3, 1] + ", " + handWorldToLocalMatrix[3, 2] + ", " + handWorldToLocalMatrix[3, 3] + ", " +
                "]]";
        }

        private Vector3 FillWithRelativeAdjacent(string boneName, Dictionary<string, Vector3> _segments)
        {
            if (gapFillingStrategy != GapFillingStrategy.FillRelative)
            {
                return Vector3.zero;
            }

            string fingerId = boneName.Substring(0, 2);

            string childName, parentName; // Thisrd wheel being the other segment of the 4 segments in the finger
            Vector3 childPos, parentPos, childPosPrevious, parentPosPrevious, segmentPosPrevious;
            segmentChild.TryGetValue(boneName, out childName);
            segmentParents.TryGetValue(boneName, out parentName);

            if (previousSegments.ContainsKey(boneName))
            {
                segmentPosPrevious = previousSegments[boneName];
                if (segmentPosPrevious == Vector3.zero)
                {
                    return Vector3.zero;
                }
            }
            else
            {
                return Vector3.zero;
            }

            if (!string.IsNullOrEmpty(childName) && _segments.ContainsKey(childName) && previousSegments.ContainsKey(childName))
            {
                childPos = _segments[childName];
                childPosPrevious = previousSegments[childName];
                if (childPos == Vector3.zero || childPosPrevious == Vector3.zero)
                {
                    return Vector3.zero;
                }
            }
            else
            {
                return Vector3.zero;
            }

            if (!string.IsNullOrEmpty(parentName) && _segments.ContainsKey(parentName) && previousSegments.ContainsKey(parentName))
            {
                parentPos = _segments[parentName];
                parentPosPrevious = previousSegments[parentName];
                if (parentPos == Vector3.zero || parentPosPrevious == Vector3.zero)
                {
                    return Vector3.zero;
                }
            }
            else
            {
                return Vector3.zero;
            }

            /// Do the actual math
            Vector3 segmentToChildVector = (childPos - segmentPosPrevious);
            Vector3 segmentToParentVector = (parentPos - segmentPosPrevious);
            float segmentToChildDistance = segmentToChildVector.magnitude;
            float segmentToParentDistance = segmentToParentVector.magnitude;

            Vector3 planePerpendicularVector;

            planePerpendicularVector = Vector3.Cross(segmentToChildVector, segmentToParentVector);

            Vector3 parentToChildVector = childPos - parentPos;
            Vector3 projectionOfSegmentFromParent = parentToChildVector * segmentToParentDistance / (segmentToParentDistance + segmentToChildDistance);
            float projectionToSegmentDistance = (float) System.Math.Sqrt((System.Math.Pow(segmentToParentDistance, 2) - System.Math.Pow(projectionOfSegmentFromParent.magnitude, 2)));
            Vector3 projectionVector = Vector3.Cross(parentToChildVector, planePerpendicularVector).normalized * projectionToSegmentDistance;

            return parentPos + projectionOfSegmentFromParent + projectionVector;
        }

        protected override void ApplyBoneTransform(Transform Bone)
        {
            string BoneName = Bone.gameObject.name;

            if (segments.ContainsKey(BoneName))
            {
                Vector3 bonePosition = segments[BoneName];

                /// Ignore setting pos/rot/scale if GapFillingStrategy.Ignore and BonePosition is zero
                /// and cannot resolve with adjacent segments (in FillRelative mode)
                if (!(gapFillingStrategy == GapFillingStrategy.Ignore || gapFillingStrategy == GapFillingStrategy.FillRelative) || bonePosition != Vector3.zero)
                {
                    if (setPosition)
                    {
                        if (BoneName == "PalmBase")
                        {
                            if (!noHand && bonePosition != Vector3.zero)
                                Bone.rotation = Quaternion.LookRotation(-bonePosition.normalized, -palm);
                            segments[BoneName] = Bone.parent.position - Bone.forward * scaleToSet - Bone.up * scaleToSet;
                        }
                        else
                        {
                            Bone.position = bonePosition * viconUnitsToUnityUnits;
                            if (segmentsRotation.TryGetValue(BoneName, out Quaternion boneRotation) && boneRotation != Quaternion.identity)
                            {
                                Bone.rotation = boneRotation;
                            }
                        }
                    }
                }
            }

            AddBoneDataToWriter(Bone);
            if (Bone.name == segment_Hand)
                handWorldToLocalMatrix = Bone.worldToLocalMatrix;
        }

        /// <inheritdoc />
        protected override void FindAndTransform(Transform iTransform, string BoneName)
        {
            base.FindAndTransform(iTransform, BoneName);
        }

        protected override bool TestSegmentsQuality(Dictionary<string, Vector3> segments)
        {
            if (segments.ContainsKey(segment_5D3) &&
                segments.ContainsKey(segment_2D3) &&
                segments.ContainsKey(segment_5D1) &&
                segments.ContainsKey(segment_2D1))
            {
                float d3_d1_dot = Vector3.Dot(segments[segment_5D3] - segments[segment_2D3], segments[segment_5D1] - segments[segment_2D1]);
                if (d3_d1_dot > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // TODO quality check when specific markers are missing
                return true;
            }
        }
    }
}
