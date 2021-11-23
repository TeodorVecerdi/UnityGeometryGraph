using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public static class GraphViewExtensions {
        public static void InsertCopyPasteData(this GraphFrameworkGraphView graphFrameworkGraphView, CopyPasteData copyPasteData) {
            if (copyPasteData == null) return;
            foreach (AbstractProperty property in copyPasteData.Properties) {
                AbstractProperty copy = property.Copy();
                graphFrameworkGraphView.GraphData.SanitizePropertyName(copy);
                graphFrameworkGraphView.GraphData.SanitizePropertyReference(copy, property.OverrideReferenceName);
                graphFrameworkGraphView.GraphData.AddProperty(copy);

                IEnumerable<SerializedNode> dependentNodes = copyPasteData.Nodes.Where(node => node.Node.IsProperty);
                foreach (SerializedNode node in dependentNodes) {
                    JObject root = JObject.Parse(node.NodeData);
                    root["propertyGuid"] = copy.GUID;
                    node.NodeData = root.ToString(Formatting.None);
                }
            }
            
            List<SerializedNode> remappedNodes = new List<SerializedNode>();
            List<SerializedEdge> remappedEdges = new List<SerializedEdge>();
            graphFrameworkGraphView.GraphData.Paste(copyPasteData, remappedNodes, remappedEdges);

            // Compute the mean of the copied nodes.
            Vector2 centroid = Vector2.zero;
            int count = 1;
            foreach (SerializedNode node in remappedNodes) {
                Vector2 position = node.DrawState.Position.position;
                centroid += (position - centroid) / count;
                ++count;
            }

            // Get the center of the current view
            Vector2 viewCenter = graphFrameworkGraphView.contentViewContainer.WorldToLocal(graphFrameworkGraphView.layout.center);

            foreach (SerializedNode node in remappedNodes) {
                NodeDrawState drawState = node.DrawState;
                Rect positionRect = drawState.Position;
                Vector2 position = positionRect.position;
                position += viewCenter - centroid;
                positionRect.position = position;
                drawState.Position = positionRect;
                node.DrawState = drawState;
            }

            graphFrameworkGraphView.ClearSelection();
            graphFrameworkGraphView.GraphData.QueueSelection(remappedNodes, remappedEdges);
        }
    }
}