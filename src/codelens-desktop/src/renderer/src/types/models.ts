export interface SolutionAnalysis {
  solutionPath: string;
  solutionName: string;
  projects: ProjectAnalysis[];
  dependencyGraph: DependencyGraph;
  architecture: ArchitectureAnalysis;
  analyzedAt: string;
}

export interface ProjectAnalysis {
  name: string;
  path: string;
  targetFramework: string;
  outputType: string;
  projectReferences: string[];
  packageReferences: PackageReference[];
  metrics: ProjectMetrics;
}

export interface ProjectMetrics {
  fileCount: number;
  totalLines: number;
  codeLines: number;
  blankLines: number;
  commentLines: number;
  classCount: number;
  methodCount: number;
  averageCyclomaticComplexity: number;
  averageMaintainabilityIndex: number;
  files: FileMetrics[];
}

export interface FileMetrics {
  filePath: string;
  totalLines: number;
  codeLines: number;
  types: TypeMetrics[];
}

export interface TypeMetrics {
  name: string;
  kind: string;
  lineCount: number;
  methodCount: number;
  maintainabilityIndex: number;
  methods: MethodMetrics[];
}

export interface MethodMetrics {
  name: string;
  lineCount: number;
  cyclomaticComplexity: number;
  classCoupling: number;
  depthOfInheritance: number;
}

export interface PackageReference {
  name: string;
  version: string;
}

export interface DependencyGraph {
  nodes: DependencyNode[];
  edges: DependencyEdge[];
}

export interface DependencyNode {
  id: string;
  label: string;
  type: string;
}

export interface DependencyEdge {
  source: string;
  target: string;
  type: string;
}

export interface ArchitectureAnalysis {
  layers: ArchitectureLayer[];
  violations: string[];
}

export interface ArchitectureLayer {
  name: string;
  projects: string[];
}

export interface HotSpot {
  filePath: string;
  commitCount: number;
  authorCount: number;
  complexity: number;
  score: number;
}
