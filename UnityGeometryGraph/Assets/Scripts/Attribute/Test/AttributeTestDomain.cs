using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Geometry;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Attribute.Test {
    public class AttributeTestDomain : MonoBehaviour {
        [SerializeField] private GeometryImporter source;
        
        [ResponsiveButtonGroup("TestsA", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Vertex -> All]")]
        private void TestA() {
            if(source == null) return;
            if (source.GeometryData == null) source.Load();

            var vertToVert = AttributeConvert.ConvertDomain(
                source.GeometryData,
                source.GeometryData.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex),
                AttributeDomain.Vertex
            ).Into<Vector3Attribute>("vert_position", AttributeDomain.Vertex);
            
            var vertToEdge = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex), 
                AttributeDomain.Edge
            ).Into<Vector3Attribute>("edge_position", AttributeDomain.Edge);
            
            var vertToFace = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex), 
                AttributeDomain.Face
            ).Into<Vector3Attribute>("face_position", AttributeDomain.Face);

            var vertToFaceCorner = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex), 
                AttributeDomain.FaceCorner
            ).Into<Vector3Attribute>("fc_position", AttributeDomain.FaceCorner);

            vertToVert.Print();
            vertToEdge.Print();
            vertToFace.Print();
            vertToFaceCorner.Print();
        }

        [ResponsiveButtonGroup("TestsA", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Edge -> All]")]
        private void TestB() {
            if(source == null) return;
            if (source.GeometryData == null) source.Load();

            var edgeToVert = AttributeConvert.ConvertDomain(
                source.GeometryData,
                source.GeometryData.GetAttribute<ClampedFloatAttribute>("crease", AttributeDomain.Edge),
                AttributeDomain.Vertex
            ).Into<ClampedFloatAttribute>("vert_crease", AttributeDomain.Vertex);
            
            var edgeToEdge = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<ClampedFloatAttribute>("crease", AttributeDomain.Edge), 
                AttributeDomain.Edge
            ).Into<ClampedFloatAttribute>("edge_crease", AttributeDomain.Edge);
            
            var edgeToFace = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<ClampedFloatAttribute>("crease", AttributeDomain.Edge), 
                AttributeDomain.Face
            ).Into<ClampedFloatAttribute>("face_crease", AttributeDomain.Face);

            var edgeToFaceCorner = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<ClampedFloatAttribute>("crease", AttributeDomain.Edge), 
                AttributeDomain.FaceCorner
            ).Into<ClampedFloatAttribute>("fc_crease", AttributeDomain.FaceCorner);

            edgeToVert.Print();
            edgeToEdge.Print();
            edgeToFace.Print();
            edgeToFaceCorner.Print();
        }
        
        [ResponsiveButtonGroup("TestsB", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Face -> All]")]
        private void TestC() {
            if(source == null) return;
            if (source.GeometryData == null) source.Load();

            var faceToVert = AttributeConvert.ConvertDomain(
                source.GeometryData,
                source.GeometryData.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face),
                AttributeDomain.Vertex
            ).Into<Vector3Attribute>("vert_normal", AttributeDomain.Vertex);
            
            var faceToEdge = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face), 
                AttributeDomain.Edge
            ).Into<Vector3Attribute>("edge_normal", AttributeDomain.Edge);
            
            var faceToFace = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face), 
                AttributeDomain.Face
            ).Into<Vector3Attribute>("face_normal", AttributeDomain.Face);

            var faceToFaceCorner = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face), 
                AttributeDomain.FaceCorner
            ).Into<Vector3Attribute>("fc_normal", AttributeDomain.FaceCorner);

            faceToVert.Print();
            faceToEdge.Print();
            faceToFace.Print();
            faceToFaceCorner.Print();
        }
        
        [ResponsiveButtonGroup("TestsB", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Face Corner -> All]")]
        private void TestD() {
            if(source == null) return;
            if (source.GeometryData == null) source.Load();

            var faceCornerToVert = AttributeConvert.ConvertDomain(
                source.GeometryData,
                source.GeometryData.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner),
                AttributeDomain.Vertex
            ).Into<Vector2Attribute>("vert_uv", AttributeDomain.Vertex);
            
            var faceCornerToEdge = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner), 
                AttributeDomain.Edge
            ).Into<Vector2Attribute>("edge_uv", AttributeDomain.Edge);
            
            var faceCornerToFace = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner), 
                AttributeDomain.Face
            ).Into<Vector2Attribute>("face_uv", AttributeDomain.Face);

            var faceCornerToFaceCorner = AttributeConvert.ConvertDomain(
                source.GeometryData, 
                source.GeometryData.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner), 
                AttributeDomain.FaceCorner
            ).Into<Vector2Attribute>("fc_uv", AttributeDomain.FaceCorner);

            faceCornerToVert.Print();
            faceCornerToEdge.Print();
            faceCornerToFace.Print();
            faceCornerToFaceCorner.Print();
        }

    }
}