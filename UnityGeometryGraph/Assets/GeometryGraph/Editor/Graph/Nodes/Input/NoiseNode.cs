using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NoiseType = GeometryGraph.Runtime.Graph.NoiseNode.NoiseNode_Type;

namespace GeometryGraph.Editor {
    [Title("Input", "Noise")]
    public class NoiseNode : AbstractNode<GeometryGraph.Runtime.Graph.NoiseNode> {
        protected override string Title => "Noise";
        protected override NodeCategory Category => NodeCategory.Input;

        private GraphFrameworkPort positionPort;
        private GraphFrameworkPort resultPort;
        private GraphFrameworkPort resultVectorPort;

        private Vector3Field positionField;
        private FloatField scaleField;
        private ClampedIntegerField octavesField;
        private ClampedFloatField lacunarityField;
        private ClampedFloatField persistenceField;
        private EnumSelectionToggle<NoiseType> typeToggle;

        private float3 position;
        private float scale = 1.0f;
        private int octaves = 4;
        private float lacunarity = 2.0f;
        private float persistence = 0.5f;
        private NoiseType type = NoiseType.Scalar;

        protected override void CreateNode() {
            (positionPort, positionField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Position", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdatePosition(position));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            resultVectorPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);

            typeToggle = new EnumSelectionToggle<NoiseType>(type);
            typeToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == type) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Noise Output Type");
                type = evt.newValue;
                RuntimeNode.UpdateType(type);
                OnTypeChanged();
            });

            positionField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(evt.newValue, position) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Noise Position");
                position = evt.newValue;
                RuntimeNode.UpdatePosition(position);
            });

            scaleField = new FloatField("Scale");
            scaleField.RegisterValueChangedCallback(evt => {
                if (math.abs(evt.newValue - scale) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Noise Scale");
                scale = evt.newValue;
                RuntimeNode.UpdateScale(scale);
            });

            octavesField = new ClampedIntegerField("Octaves", 1, Constants.MAX_NOISE_OCTAVES);
            octavesField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == octaves) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Noise Octaves");
                octaves = evt.newValue;
                RuntimeNode.UpdateOctaves(octaves);
            });

            lacunarityField = new ClampedFloatField("Lacunarity", 0.001f);
            lacunarityField.RegisterValueChangedCallback(evt => {
                if (math.abs(evt.newValue - lacunarity) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Noise Lacunarity");
                lacunarity = evt.newValue;
                RuntimeNode.UpdateLacunarity(lacunarity);
            });

            persistenceField = new ClampedFloatField("Persistence", 0.001f);
            persistenceField.RegisterValueChangedCallback(evt => {
                if (math.abs(evt.newValue - persistence) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Noise Persistence");
                persistence = evt.newValue;
                RuntimeNode.UpdatePersistence(persistence);
            });

            typeToggle.SetValueWithoutNotify(type);
            scaleField.SetValueWithoutNotify(scale);
            octavesField.SetValueWithoutNotify(octaves);
            lacunarityField.SetValueWithoutNotify(lacunarity);
            persistenceField.SetValueWithoutNotify(persistence);

            inputContainer.Add(typeToggle);
            inputContainer.Add(octavesField);
            inputContainer.Add(scaleField);
            inputContainer.Add(lacunarityField);
            inputContainer.Add(persistenceField);
            AddPort(positionPort);
            inputContainer.Add(positionField);
            AddPort(resultPort);
            AddPort(resultVectorPort);

            OnTypeChanged();
        }

        private void OnTypeChanged() {
            if (type == NoiseType.Scalar) {
                resultPort.Show();
                resultVectorPort.HideAndDisconnect();
            } else {
                resultPort.HideAndDisconnect();
                resultVectorPort.Show();
            }
        }

        protected override void BindPorts() {
            BindPort(positionPort, RuntimeNode.PositionPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
            BindPort(resultVectorPort, RuntimeNode.ResultVectorPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray data = new() {
                (int)type,
                scale,
                octaves,
                lacunarity,
                persistence,
                JsonConvert.SerializeObject(position, float3Converter.Converter),
            };
            root["d"] = data;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            type = (NoiseType)array!.Value<int>(0);
            scale = array.Value<float>(1);
            octaves = array.Value<int>(2);
            lacunarity = array.Value<float>(3);
            persistence = array.Value<float>(4);
            position = JsonConvert.DeserializeObject<float3>(array.Value<string>(5), float3Converter.Converter);

            typeToggle.SetValueWithoutNotify(type);
            scaleField.SetValueWithoutNotify(scale);
            octavesField.SetValueWithoutNotify(octaves);
            lacunarityField.SetValueWithoutNotify(lacunarity);
            persistenceField.SetValueWithoutNotify(persistence);
            positionField.SetValueWithoutNotify(position);

            RuntimeNode.UpdateType(type);
            RuntimeNode.UpdateScale(scale);
            RuntimeNode.UpdateOctaves(octaves);
            RuntimeNode.UpdateLacunarity(lacunarity);
            RuntimeNode.UpdatePersistence(persistence);
            RuntimeNode.UpdatePosition(position);

            OnTypeChanged();

            base.Deserialize(data);
        }
    }
}