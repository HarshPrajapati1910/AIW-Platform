from langgraph.graph import StateGraph, START, END
from app.graph.state import ReviewGraphState
from app.graph.agents.supervisor_agent import SupervisorAgent
from app.graph.agents.security_agent import SecuritySpecialistAgent
from app.graph.agents.performance_agent import PerformanceSpecialistAgent
from app.graph.agents.architecture_agent import ArchitectureSpecialistAgent
from app.graph.agents.synthesis_agent import SynthesisScoringAgent

class ReviewGraphBuilder:
    """
    Constructs an explicit, decoupled StateGraph where agents execute 
    in parallel branches and converge into the synthesis node.
    """
    @staticmethod
    def build():
        supervisor = SupervisorAgent()
        security = SecuritySpecialistAgent()
        performance = PerformanceSpecialistAgent()
        architecture = ArchitectureSpecialistAgent()
        synthesis = SynthesisScoringAgent()

        builder = StateGraph(ReviewGraphState)

        # Register independent agent nodes
        builder.add_node("supervisor", supervisor.run)
        builder.add_node("security_node", security.run)
        builder.add_node("performance_node", performance.run)
        builder.add_node("architecture_node", architecture.run)
        builder.add_node("synthesis_node", synthesis.run)

        # Wire pipeline dependencies:
        # Start -> Supervisor
        builder.add_edge(START, "supervisor")

        # Fan-out: Parallel specialist evaluation
        builder.add_edge("supervisor", "security_node")
        builder.add_edge("supervisor", "performance_node")
        builder.add_edge("supervisor", "architecture_node")

        # Fan-in: Converge into Synthesis
        builder.add_edge("security_node", "synthesis_node")
        builder.add_edge("performance_node", "synthesis_node")
        builder.add_edge("architecture_node", "synthesis_node")

        # Synthesis -> End
        builder.add_edge("synthesis_node", END)

        return builder.compile()


review_graph = ReviewGraphBuilder.build()