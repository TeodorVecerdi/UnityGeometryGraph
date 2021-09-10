using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Attribute.Test {
    public class AttributeTest : MonoBehaviour {
        [ResponsiveButtonGroup("TestsA", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Simple]\nSame length, Same type")]
        private void TestSimple() {
            var boolA = new BoolAttribute("some_attribute", new[] { true, false, true, false }) { Domain = AttributeDomain.Vertex };
            var boolB = new BoolAttribute("some_attribute2", new[] { false, true, true, false }) { Domain = AttributeDomain.Vertex };

            var boolC = boolA.YieldWithAttribute(boolB, (valueA, valueB) => valueA || valueB).Into<BoolAttribute>("other_attribute", AttributeDomain.Vertex);
            var boolD = boolA.YieldWithAttribute(boolB, (valueA, valueB) => valueA && valueB).Into<BoolAttribute>("other_attribute2", AttributeDomain.Vertex);
            var boolE = boolA.YieldWithAttribute(boolA, (b1, b2) => b1 && b2).Into<BoolAttribute>("other_attribute3", AttributeDomain.Vertex);

            boolC.Print();
            boolD.Print();
            boolE.Print();
        }

        [ResponsiveButtonGroup("TestsA", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Conversion]\nBool <-> Float")]
        private void TestBoolToFloat() {
            var boolAttr = new BoolAttribute("bool_attribute", new[] { true, false, true, false }) { Domain = AttributeDomain.Vertex };
            var floatAttr = new FloatAttribute("float_attribute", new[] { 0.0f, 1.0f, 2.0f, 3.0f }) { Domain = AttributeDomain.Vertex };

            var bool2float = floatAttr.YieldWithAttribute(boolAttr, (f1, f2) => f1 + f2).Into<FloatAttribute>("bool_to_float", AttributeDomain.Vertex);
            var float2bool = boolAttr.YieldWithAttribute(floatAttr, (b1, b2) => b1 && b2).Into("float_to_bool", AttributeDomain.Vertex, typeof(BoolAttribute));

            bool2float.Print();
            float2bool.Print();
        }
        
        [ResponsiveButtonGroup("TestsB", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Conversion]\nInt, Float -> ClampedFloat ")]
        private void TestToClampedFloat() {
            var intAttr = new IntAttribute("int_attribute", new[] { -2, 0, 2, 100 }) { Domain = AttributeDomain.Vertex };
            var floatAttr = new FloatAttribute("float_attribute", new[] { -0.5f, 0.125f, 1.0f, 25.0f }) { Domain = AttributeDomain.Vertex };

            var clampedA = floatAttr.Into<ClampedFloatAttribute>("clampedA");
            var clampedB = intAttr.Into<ClampedFloatAttribute>("clampedB");
            
            clampedA.Print();
            clampedB.Print();
        }

        [ResponsiveButtonGroup("TestsB", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Chained]")]
        private void TestChained() {
            var floatAttr = new FloatAttribute("float_attribute", new[] { 0.0f, 1.0f, 2.0f, 3.0f }) { Domain = AttributeDomain.Vertex };
            var floatAttr2 = new FloatAttribute("float_attribute2", new[] { 1.0f, 1.0f, 3.0f, 3.0f }) { Domain = AttributeDomain.Vertex };
            var floatAttr3 = new FloatAttribute("float_attribute3", new[] { 1.0f, 0.0f, 0.0f, 0.0f }) { Domain = AttributeDomain.Vertex };

            var newFloatAttr = floatAttr
                               .Yield(value => value * 2.0f)
                               .YieldWithAttribute(floatAttr.Type, floatAttr2, (f1, f2) => f1 + f2)
                               .YieldWithAttribute(floatAttr.Type, floatAttr3, (f1, f2) => f1 * f2)
                               .Into<FloatAttribute>("new_attribute", AttributeDomain.Vertex);

            newFloatAttr.Print();
        }
        
        [ResponsiveButtonGroup("TestsC", UniformLayout = true, DefaultButtonSize = ButtonSizes.Gigantic), Button(ButtonHeight = 48, Name = "[Different Sizes]")]
        private void TestDifferentSizes() {
            var floatAttr = new FloatAttribute("float_attribute", new[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f }) { Domain = AttributeDomain.Vertex };
            var floatAttr2 = new FloatAttribute("float_attribute2", new[] { 10.0f, 10.0f, 10.0f }) { Domain = AttributeDomain.Vertex };
            var floatAttr3 = new FloatAttribute("float_attribute3", Array.Empty<float>()) { Domain = AttributeDomain.Vertex };

            var fA = floatAttr.YieldWithAttribute(floatAttr2, (f1, f2) => f1 + f2).Into<FloatAttribute>("new1", AttributeDomain.Vertex);
            var fB = floatAttr2.YieldWithAttribute(floatAttr, (f1, f2) => f1 - f2).Into<FloatAttribute>("new2", AttributeDomain.Vertex);
            var fC = floatAttr2.YieldWithAttribute(floatAttr3, (f1, f2) => f1 * f2).Into<FloatAttribute>("new3", AttributeDomain.Vertex);
            
            fA.Print();
            fB.Print();
            fC.Print();
        }
    }
}