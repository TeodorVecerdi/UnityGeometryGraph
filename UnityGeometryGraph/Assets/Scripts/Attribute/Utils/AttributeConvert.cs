using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityCommons;
using UnityEngine;

namespace Attribute {
    internal static class AttributeConvert {
        
        internal static bool TryGetType(object value, out AttributeType type) {
            var valueType = value.GetType();
            type = typeConversionDictionary.ContainsKey(valueType) ? typeConversionDictionary[valueType] : AttributeType.Invalid;
            return type != AttributeType.Invalid;
        }

        // oof sorry 
        internal static T ConvertType<T>(object value, AttributeType from, AttributeType to) {
            return from switch {
                AttributeType.Boolean => to switch {
                    AttributeType.Boolean => (T)value,
                    AttributeType.Integer => (T)ConvertType_BoolInt((bool)value),
                    AttributeType.Float => (T)ConvertType_BoolFloat((bool)value),
                    AttributeType.ClampedFloat => (T)ConvertType_BoolClampedFloat((bool)value),
                    AttributeType.Vector2 => (T)ConvertType_BoolVec2((bool)value),
                    AttributeType.Vector3 => (T)ConvertType_BoolVec3((bool)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Integer => to switch {
                    AttributeType.Boolean => (T)ConvertType_IntBool((int)value),
                    AttributeType.Integer => (T)value,
                    AttributeType.Float => (T)ConvertType_IntFloat((int)value),
                    AttributeType.ClampedFloat => (T)ConvertType_IntClampedFloat((int)value),
                    AttributeType.Vector2 => (T)ConvertType_IntVec2((int)value),
                    AttributeType.Vector3 => (T)ConvertType_IntVec3((int)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Float => to switch {
                    AttributeType.Boolean => (T)ConvertType_FloatBool((float)value),
                    AttributeType.Integer => (T)ConvertType_FloatInt((float)value),
                    AttributeType.Float => (T)value,
                    AttributeType.ClampedFloat => (T)ConvertType_FloatClampedFloat((float)value),
                    AttributeType.Vector2 => (T)ConvertType_FloatVec2((float)value),
                    AttributeType.Vector3 => (T)ConvertType_FloatVec3((float)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.ClampedFloat => to switch {
                    AttributeType.Boolean => (T)ConvertType_ClampedFloatBool((float)value),
                    AttributeType.Integer => (T)ConvertType_ClampedFloatInt((float)value),
                    AttributeType.Float => (T)ConvertType_ClampedFloatFloat((float)value),
                    AttributeType.ClampedFloat => (T)value,
                    AttributeType.Vector2 => (T)ConvertType_ClampedFloatVec2((float)value),
                    AttributeType.Vector3 => (T)ConvertType_ClampedFloatVec3((float)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Vector2 => to switch {
                    AttributeType.Boolean => (T)ConvertType_Vec2Bool((Vector2)value),
                    AttributeType.Integer => (T)ConvertType_Vec2Int((Vector2)value),
                    AttributeType.Float => (T)ConvertType_Vec2Float((Vector2)value),
                    AttributeType.ClampedFloat => (T)ConvertType_Vec2ClampedFloat((Vector2)value),
                    AttributeType.Vector2 => (T)value,
                    AttributeType.Vector3 => (T)ConvertType_Vec2Vec3((Vector2)value),
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                AttributeType.Vector3 => to switch {
                    AttributeType.Boolean => (T)ConvertType_Vec3Bool((Vector3)value),
                    AttributeType.Integer => (T)ConvertType_Vec3Int((Vector3)value),
                    AttributeType.Float => (T)ConvertType_Vec3Float((Vector3)value),
                    AttributeType.ClampedFloat => (T)ConvertType_Vec3ClampedFloat((Vector3)value),
                    AttributeType.Vector2 => (T)ConvertType_Vec3Vec2((Vector3)value),
                    AttributeType.Vector3 => (T)value,
                    _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
            };
        }

        // Boolean
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolInt(bool a) => a ? 1 : 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolFloat(bool a) => a ? 1.0f : 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolClampedFloat(bool a) => a ? 1.0f : 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolVec2(bool a) => a ? Vector2.one : Vector2.zero;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_BoolVec3(bool a) => a ? Vector3.one : Vector3.zero;

        // Integer
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntBool(int a) => a != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntFloat(int a) => a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntClampedFloat(int a) => a.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntVec2(int a) => Vector2.one * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_IntVec3(int a) => Vector3.one * a;

        // Float
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatBool(float a) => a != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatInt(float a) => (int)a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatClampedFloat(float a) => a.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatVec2(float a) => Vector2.one * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_FloatVec3(float a) => Vector3.one * a;

        // Clamped Float
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatBool(float a) => a != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatInt(float a) => Mathf.Approximately(a, 1.0f) ? 1 : 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatFloat(float a) => a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatVec2(float a) => Vector2.one * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_ClampedFloatVec3(float a) => Vector3.one * a;

        // Vector2
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Bool(Vector2 a) => a.x != 0.0f && a.y != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Int(Vector2 a) => (int)a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Float(Vector2 a) => a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2ClampedFloat(Vector2 a) => a.x.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec2Vec3(Vector2 a) => a;

        // Vector3
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Bool(Vector3 a) => a.x != 0.0f && a.y != 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Int(Vector3 a) => (int)a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Float(Vector3 a) => a.x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3ClampedFloat(Vector3 a) => a.x.Clamped01();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static object ConvertType_Vec3Vec2(Vector3 a) => a;


        private static readonly Dictionary<Type, AttributeType> typeConversionDictionary = new Dictionary<Type, AttributeType> {
            { typeof(bool), AttributeType.Boolean },
            { typeof(int), AttributeType.Integer },
            { typeof(float), AttributeType.Float },
            { typeof(Vector2), AttributeType.Vector2 },
            { typeof(Vector3), AttributeType.Vector3 },
        };
    }
}