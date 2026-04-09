// Bar chart for LOC per project
window.renderBarChart = function (elementId, data) {
    const container = document.getElementById(elementId);
    if (!container) return;
    container.innerHTML = '';

    const margin = { top: 20, right: 30, bottom: 60, left: 60 };
    const width = container.clientWidth - margin.left - margin.right;
    const height = 280 - margin.top - margin.bottom;

    const svg = d3.select(`#${elementId}`)
        .append('svg')
        .attr('width', width + margin.left + margin.right)
        .attr('height', height + margin.top + margin.bottom)
        .append('g')
        .attr('transform', `translate(${margin.left},${margin.top})`);

    const x = d3.scaleBand().domain(data.map(d => d.label)).range([0, width]).padding(0.3);
    const y = d3.scaleLinear().domain([0, d3.max(data, d => d.value) * 1.1]).range([height, 0]);

    svg.append('g')
        .attr('transform', `translate(0,${height})`)
        .call(d3.axisBottom(x))
        .selectAll('text')
        .attr('transform', 'rotate(-25)')
        .style('text-anchor', 'end')
        .style('fill', '#8b949e')
        .style('font-size', '11px');

    svg.append('g')
        .call(d3.axisLeft(y).ticks(5))
        .selectAll('text')
        .style('fill', '#8b949e');

    svg.selectAll('.bar')
        .data(data)
        .enter()
        .append('rect')
        .attr('x', d => x(d.label))
        .attr('y', d => y(d.value))
        .attr('width', x.bandwidth())
        .attr('height', d => height - y(d.value))
        .attr('fill', '#58a6ff')
        .attr('rx', 3)
        .on('mouseover', function () { d3.select(this).attr('fill', '#79c0ff'); })
        .on('mouseout', function () { d3.select(this).attr('fill', '#58a6ff'); });

    // Value labels
    svg.selectAll('.label')
        .data(data)
        .enter()
        .append('text')
        .attr('x', d => x(d.label) + x.bandwidth() / 2)
        .attr('y', d => y(d.value) - 5)
        .attr('text-anchor', 'middle')
        .attr('fill', '#8b949e')
        .style('font-size', '11px')
        .text(d => d.value.toLocaleString());

    svg.selectAll('path, line').attr('stroke', '#30363d');
};

// Force-directed graph for dependencies
window.renderForceGraph = function (elementId, nodes, edges) {
    const container = document.getElementById(elementId);
    if (!container) return;
    container.innerHTML = '';

    const width = container.clientWidth;
    const height = 500;

    const svg = d3.select(`#${elementId}`)
        .append('svg')
        .attr('width', width)
        .attr('height', height);

    const g = svg.append('g');

    // Zoom
    svg.call(d3.zoom().scaleExtent([0.3, 3]).on('zoom', (event) => {
        g.attr('transform', event.transform);
    }));

    const nodesArray = Array.from(nodes);
    const edgesArray = Array.from(edges).map(e => ({ ...e }));

    const simulation = d3.forceSimulation(nodesArray)
        .force('link', d3.forceLink(edgesArray).id(d => d.id).distance(120))
        .force('charge', d3.forceManyBody().strength(-400))
        .force('center', d3.forceCenter(width / 2, height / 2))
        .force('collision', d3.forceCollide().radius(35));

    // Edges
    const link = g.append('g')
        .selectAll('line')
        .data(edgesArray)
        .enter()
        .append('line')
        .attr('stroke', d => d.type === 'reference' ? '#58a6ff' : '#30363d')
        .attr('stroke-width', d => d.type === 'reference' ? 2 : 1)
        .attr('stroke-dasharray', d => d.type === 'package' ? '4,4' : null)
        .attr('marker-end', 'url(#arrow)');

    // Arrow marker
    svg.append('defs').append('marker')
        .attr('id', 'arrow')
        .attr('viewBox', '0 0 10 10')
        .attr('refX', 20)
        .attr('refY', 5)
        .attr('markerWidth', 6)
        .attr('markerHeight', 6)
        .attr('orient', 'auto')
        .append('path')
        .attr('d', 'M 0 0 L 10 5 L 0 10 z')
        .attr('fill', '#58a6ff');

    // Nodes
    const node = g.append('g')
        .selectAll('circle')
        .data(nodesArray)
        .enter()
        .append('circle')
        .attr('r', d => d.type === 'project' ? 12 : 6)
        .attr('fill', d => d.type === 'project' ? '#58a6ff' : '#3fb950')
        .attr('stroke', '#f0f6fc')
        .attr('stroke-width', 1.5)
        .call(d3.drag()
            .on('start', (event, d) => { if (!event.active) simulation.alphaTarget(0.3).restart(); d.fx = d.x; d.fy = d.y; })
            .on('drag', (event, d) => { d.fx = event.x; d.fy = event.y; })
            .on('end', (event, d) => { if (!event.active) simulation.alphaTarget(0); d.fx = null; d.fy = null; }));

    // Labels
    const label = g.append('g')
        .selectAll('text')
        .data(nodesArray)
        .enter()
        .append('text')
        .attr('dy', d => d.type === 'project' ? -18 : -10)
        .attr('text-anchor', 'middle')
        .attr('fill', '#f0f6fc')
        .style('font-size', d => d.type === 'project' ? '12px' : '9px')
        .text(d => d.label);

    simulation.on('tick', () => {
        link.attr('x1', d => d.source.x).attr('y1', d => d.source.y)
            .attr('x2', d => d.target.x).attr('y2', d => d.target.y);
        node.attr('cx', d => d.x).attr('cy', d => d.y);
        label.attr('x', d => d.x).attr('y', d => d.y);
    });
};

// Scatter plot for hot spots
window.renderScatterPlot = function (elementId, data) {
    const container = document.getElementById(elementId);
    if (!container) return;
    container.innerHTML = '';

    const margin = { top: 20, right: 30, bottom: 50, left: 60 };
    const width = container.clientWidth - margin.left - margin.right;
    const height = 300 - margin.top - margin.bottom;

    const svg = d3.select(`#${elementId}`)
        .append('svg')
        .attr('width', width + margin.left + margin.right)
        .attr('height', height + margin.top + margin.bottom)
        .append('g')
        .attr('transform', `translate(${margin.left},${margin.top})`);

    const dataArray = Array.from(data);
    const x = d3.scaleLinear()
        .domain([0, d3.max(dataArray, d => d.commits) * 1.1])
        .range([0, width]);

    const y = d3.scaleLinear()
        .domain([0, d3.max(dataArray, d => d.complexity) * 1.1])
        .range([height, 0]);

    // Danger zone (upper-right)
    const midX = d3.max(dataArray, d => d.commits) * 0.5;
    const midY = d3.max(dataArray, d => d.complexity) * 0.5;
    svg.append('rect')
        .attr('x', x(midX)).attr('y', 0)
        .attr('width', width - x(midX)).attr('height', y(midY))
        .attr('fill', 'rgba(248, 81, 73, 0.08)');

    svg.append('g').attr('transform', `translate(0,${height})`).call(d3.axisBottom(x).ticks(5))
        .selectAll('text').style('fill', '#8b949e');
    svg.append('g').call(d3.axisLeft(y).ticks(5))
        .selectAll('text').style('fill', '#8b949e');

    // Axis labels
    svg.append('text').attr('x', width / 2).attr('y', height + 40)
        .attr('text-anchor', 'middle').attr('fill', '#8b949e').style('font-size', '12px')
        .text('Commits (Churn)');
    svg.append('text').attr('transform', 'rotate(-90)').attr('x', -height / 2).attr('y', -45)
        .attr('text-anchor', 'middle').attr('fill', '#8b949e').style('font-size', '12px')
        .text('Complexity');

    // Dots
    const tooltip = d3.select(`#${elementId}`).append('div')
        .style('position', 'absolute').style('background', '#21262d')
        .style('border', '1px solid #30363d').style('border-radius', '4px')
        .style('padding', '6px 10px').style('font-size', '12px')
        .style('color', '#f0f6fc').style('pointer-events', 'none')
        .style('opacity', 0);

    svg.selectAll('circle')
        .data(dataArray)
        .enter()
        .append('circle')
        .attr('cx', d => x(d.commits))
        .attr('cy', d => y(d.complexity))
        .attr('r', d => Math.max(4, Math.min(12, d.score / 10)))
        .attr('fill', d => d.score > 80 ? '#f85149' : d.score > 30 ? '#d29922' : '#3fb950')
        .attr('opacity', 0.8)
        .on('mouseover', function (event, d) {
            tooltip.style('opacity', 1)
                .html(`<strong>${d.file}</strong><br>Commits: ${d.commits} | CC: ${d.complexity.toFixed(1)} | Score: ${d.score.toFixed(1)}`)
                .style('left', (event.offsetX + 10) + 'px')
                .style('top', (event.offsetY - 10) + 'px');
            d3.select(this).attr('stroke', '#f0f6fc').attr('stroke-width', 2);
        })
        .on('mouseout', function () {
            tooltip.style('opacity', 0);
            d3.select(this).attr('stroke', null);
        });

    svg.selectAll('path, line').attr('stroke', '#30363d');
};
