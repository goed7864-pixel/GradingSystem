namespace GradingSystem.Services
{
    public class ServiceLocator
    {
        private static ServiceLocator? _instance;
        private readonly ApiClient _apiClient;

        public AuthService AuthService { get; }
        public UserService UserService { get; }
        public CourseService CourseService { get; }
        public AssignmentService AssignmentService { get; }
        public SubmissionService SubmissionService { get; }
        public GradeService GradeService { get; }
        public GroupService GroupService { get; }
        public EnrollmentService EnrollmentService { get; }
        public DashboardService DashboardService { get; }
        public TokenService TokenService { get; }

        private ServiceLocator()
        {
            _apiClient = new ApiClient();
            TokenService = new TokenService();
            AuthService = new AuthService(_apiClient, TokenService);
            UserService = new UserService(_apiClient);
            CourseService = new CourseService(_apiClient);
            AssignmentService = new AssignmentService(_apiClient);
            SubmissionService = new SubmissionService(_apiClient);
            GradeService = new GradeService(_apiClient);
            GroupService = new GroupService(_apiClient);
            EnrollmentService = new EnrollmentService(_apiClient);
            DashboardService = new DashboardService(_apiClient);
        }

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }
                return _instance;
            }
        }
    }
}
