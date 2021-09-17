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
            foreach (var property in copyPasteData.Properties) {
                var copy = property.Copy();
                graphFrameworkGraphView.GraphData.SanitizePropertyName(copy);
                graphFrameworkGraphView.GraphData.SanitizePropertyReference(copy, property.OverrideReferenceName);
                graphFrameworkGraphView.GraphData.AddProperty(copy);

                var dependentNodes = copyPasteData.Nodes.Where(node => node.Node.IsProperty);
                foreach (var node in dependentNodes) {
                    var root = JObject.Parse(node.NodeData);
                    root["propertyGuid"] = copy.GUID;
                    node.NodeData = root.ToString(Formatting.None);
                }
            }
            
            var remappedNodes = new List<SerializedNode>();
            var remappedEdges = new List<SerializedEdge>();
            graphFrameworkGraphView.GraphData.Paste(copyPasteData, remappedNodes, remappedEdges);

            // Compute the mean of the copied nodes.
            var centroid = Vector2.zero;
            var count = 1;
            foreach (var node in remappedNodes) {
                var position = node.DrawState.Position.position;
                centroid += (position - centroid) / count;
                ++count;
            }

            // Get the center of the current view
            var viewCenter = graphFrameworkGraphView.contentViewContainer.WorldToLocal(graphFrameworkGraphView.layout.center);

            foreach (var node in remappedNodes) {
                var drawState = node.DrawState;
                var positionRect = drawState.Position;
                var position = positionRect.position;
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