import { useRef, useEffect } from 'react';
import * as d3 from 'd3';
import type { DependencyNode, DependencyEdge } from '../../types/models';

interface Props {
  nodes: DependencyNode[];
  edges: DependencyEdge[];
}

interface SimNode extends d3.SimulationNodeDatum {
  id: string;
  label: string;
  type: string;
}

interface SimLink extends d3.SimulationLinkDatum<SimNode> {
  type: string;
}

export default function ForceGraph({ nodes, edges }: Props) {
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || nodes.length === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll('*').remove();

    const width = svgRef.current.clientWidth;
    const height = 500;

    const simNodes: SimNode[] = nodes.map(n => ({ ...n }));
    const simLinks: SimLink[] = edges.map(e => ({ source: e.source, target: e.target, type: e.type }));

    const g = svg.append('g');
    svg.call(d3.zoom<SVGSVGElement, unknown>().scaleExtent([0.3, 3]).on('zoom', (event) => {
      g.attr('transform', event.transform);
    }) as never);

    // Arrow marker
    svg.append('defs').append('marker')
      .attr('id', 'arrow').attr('viewBox', '0 0 10 10')
      .attr('refX', 20).attr('refY', 5).attr('markerWidth', 6).attr('markerHeight', 6)
      .attr('orient', 'auto')
      .append('path').attr('d', 'M 0 0 L 10 5 L 0 10 z').attr('fill', '#58a6ff');

    const simulation = d3.forceSimulation(simNodes)
      .force('link', d3.forceLink<SimNode, SimLink>(simLinks).id(d => d.id).distance(120))
      .force('charge', d3.forceManyBody().strength(-400))
      .force('center', d3.forceCenter(width / 2, height / 2))
      .force('collision', d3.forceCollide().radius(35));

    const link = g.append('g').selectAll('line').data(simLinks).enter().append('line')
      .attr('stroke', d => d.type === 'reference' ? '#58a6ff' : '#30363d')
      .attr('stroke-width', d => d.type === 'reference' ? 2 : 1)
      .attr('stroke-dasharray', d => d.type === 'package' ? '4,4' : null)
      .attr('marker-end', 'url(#arrow)');

    const node = g.append('g').selectAll('circle').data(simNodes).enter().append('circle')
      .attr('r', d => d.type === 'project' ? 12 : 6)
      .attr('fill', d => d.type === 'project' ? '#58a6ff' : '#3fb950')
      .attr('stroke', '#f0f6fc').attr('stroke-width', 1.5)
      .call(d3.drag<SVGCircleElement, SimNode>()
        .on('start', (event, d) => { if (!event.active) simulation.alphaTarget(0.3).restart(); d.fx = d.x; d.fy = d.y; })
        .on('drag', (event, d) => { d.fx = event.x; d.fy = event.y; })
        .on('end', (event, d) => { if (!event.active) simulation.alphaTarget(0); d.fx = null; d.fy = null; }));

    const label = g.append('g').selectAll('text').data(simNodes).enter().append('text')
      .attr('dy', d => d.type === 'project' ? -18 : -10)
      .attr('text-anchor', 'middle').attr('fill', '#f0f6fc')
      .style('font-size', d => d.type === 'project' ? '12px' : '9px')
      .text(d => d.label);

    simulation.on('tick', () => {
      link.attr('x1', d => (d.source as SimNode).x!).attr('y1', d => (d.source as SimNode).y!)
        .attr('x2', d => (d.target as SimNode).x!).attr('y2', d => (d.target as SimNode).y!);
      node.attr('cx', d => d.x!).attr('cy', d => d.y!);
      label.attr('x', d => d.x!).attr('y', d => d.y!);
    });

    return () => { simulation.stop(); };
  }, [nodes, edges]);

  return <svg ref={svgRef} width="100%" height="500" />;
}
