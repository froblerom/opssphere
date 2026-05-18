namespace OpsSphere.Infrastructure.Persistence.SeedData;

public static class SeedIds
{
    public static class Roles
    {
        public static readonly Guid Admin = Guid.Parse("e66bb345-2b5b-4f87-b106-31091edebea9");
        public static readonly Guid OperationsManager = Guid.Parse("1ed479f8-9720-45cb-aad9-ce94cda99dce");
        public static readonly Guid Supervisor = Guid.Parse("f486f23d-c0a7-4b37-912f-3511d3e18a0f");
        public static readonly Guid Agent = Guid.Parse("0d960057-6c2e-43e3-a1ed-9b219a19b6bd");
        public static readonly Guid Viewer = Guid.Parse("aebad384-eff3-4069-886a-1621c5b21e9c");
    }

    public static class Permissions
    {
        public static readonly Guid UsersView = Guid.Parse("e518b7bb-dbc9-40c2-b7fc-13c7bb13b23f");
        public static readonly Guid UsersManage = Guid.Parse("38bdf4a8-0d42-4ecd-9f40-e7c3cc541226");
        public static readonly Guid RolesView = Guid.Parse("4733e568-a0db-4e84-b8ad-6dcc46e5488e");
        public static readonly Guid RolesManage = Guid.Parse("8f295e14-49c1-4c2a-9d63-0e4a5a9cc9b2");
        public static readonly Guid PermissionsView = Guid.Parse("cfbee537-28b0-4de8-b688-40c16d76e0ab");
        public static readonly Guid PermissionsManage = Guid.Parse("f0a054d6-3159-4442-a37e-1518069c4389");
        public static readonly Guid ScopesView = Guid.Parse("b3f01126-7532-4d53-8c0f-8bc05bd856cc");
        public static readonly Guid ScopesManage = Guid.Parse("f7c63236-1068-42fb-8341-f91fb1a97327");
        public static readonly Guid OrganizationView = Guid.Parse("f2d85ff0-5e8e-431f-a9ea-78dc39b7f84c");
        public static readonly Guid OrganizationManage = Guid.Parse("1233c870-529a-4f3d-9b61-85199d93ddf8");
        public static readonly Guid RegionsManage = Guid.Parse("00fd5a1a-e3a9-44e3-8642-f8bf3f3e9da9");
        public static readonly Guid CountriesManage = Guid.Parse("c8973bc5-c8a8-46eb-9745-1234ac8c504f");
        public static readonly Guid AccountsManage = Guid.Parse("6f63fb7a-88c2-4afb-9982-39a3075fd44f");
        public static readonly Guid CampaignsManage = Guid.Parse("cd00a1bf-166a-40c1-9c55-e05e33250680");
        public static readonly Guid AssignmentsManage = Guid.Parse("291eb57c-b84d-42a1-a87a-079a6594309b");
        public static readonly Guid CustomersView = Guid.Parse("c01dec64-a682-41ea-b55a-33d535812ee3");
        public static readonly Guid CustomersCreate = Guid.Parse("e79074c8-d245-46be-8401-a7820281840e");
        public static readonly Guid CustomersUpdate = Guid.Parse("f157982a-ac66-4c80-b134-678591c013e2");
        public static readonly Guid CustomersHistoryView = Guid.Parse("12458f20-62b5-45ad-85a6-0e975dab1434");
        public static readonly Guid TicketsView = Guid.Parse("7fdc43ea-03b5-4efd-826f-da7c68978c61");
        public static readonly Guid TicketsCreate = Guid.Parse("2f07dc1e-3160-4aed-9246-5a1848363a3d");
        public static readonly Guid TicketsUpdate = Guid.Parse("2880b151-a2fa-4707-a570-7847c373ac3f");
        public static readonly Guid TicketsAssign = Guid.Parse("19cc4d2e-ed33-4541-bd03-9cc049594c5a");
        public static readonly Guid TicketsUpdateStatus = Guid.Parse("b2fee76e-7cc3-4f10-9577-e5a3329ebc6f");
        public static readonly Guid TicketsUpdatePriority = Guid.Parse("22f325ae-f416-48d0-9ba8-f40828ff7eaf");
        public static readonly Guid TicketsComment = Guid.Parse("30ca5af8-7934-4ff0-85e0-e848a73bd09b");
        public static readonly Guid TicketsEscalate = Guid.Parse("1fae5562-1478-4dbf-bf12-c42df2c8a35d");
        public static readonly Guid TicketsResolve = Guid.Parse("21479de7-9fbe-44d0-a6fa-80631aec147f");
        public static readonly Guid TicketsClose = Guid.Parse("b0e0cd5a-486b-4286-8d96-e2bd3950cb74");
        public static readonly Guid TicketsHistoryView = Guid.Parse("94403333-a65b-4455-9d2b-55d4c36570a5");
        public static readonly Guid TicketsReopen = Guid.Parse("c2c34198-9d7a-45e8-8414-ff17b059abd9");
        public static readonly Guid SlaView = Guid.Parse("3a566513-d148-40a7-92ca-26cbcf0eda64");
        public static readonly Guid SlaPoliciesView = Guid.Parse("d4b944f8-1c92-4340-964c-ac55c73f0583");
        public static readonly Guid SlaPoliciesManage = Guid.Parse("f7f32bfd-2d73-4d22-9522-2ed15b8133e7");
        public static readonly Guid SlaEvaluate = Guid.Parse("8d10f3f8-10ba-4cdd-9bdc-e7179244fd2e");
        public static readonly Guid DashboardView = Guid.Parse("ea4c7b44-5675-4a33-9e41-46023b6fb71a");
        public static readonly Guid ReportsView = Guid.Parse("c40d5d7b-0ab1-41ac-a77f-517bdcfb2392");
        public static readonly Guid ReportsExport = Guid.Parse("e77c60be-611c-4d3f-b672-97116bdde705");
        public static readonly Guid AuditView = Guid.Parse("8dc2ff39-d9e1-4934-9ae5-fe8db683d397");
        public static readonly Guid AuditAdminView = Guid.Parse("9ee63109-a8fb-4269-9dd8-3d8804b323a0");
        public static readonly Guid AuditExport = Guid.Parse("70dbdb82-df99-4d97-ae78-3e9178369925");
    }

    public static class Users
    {
        public static readonly Guid Admin = Guid.Parse("cdf3edb1-fee0-4bad-96b6-e26b670e86aa");
        public static readonly Guid ManagerLatam = Guid.Parse("a9b6f62c-8b2b-4906-b7e2-59ae759642e2");
        public static readonly Guid SupervisorNovabank = Guid.Parse("69af963e-f944-450c-af3e-06fca1866f52");
        public static readonly Guid AgentNovabank = Guid.Parse("de55f444-bea5-485e-a348-6909ea378422");
        public static readonly Guid ViewerLatam = Guid.Parse("abfb2ab0-4b94-49cd-bebe-35ce548972ec");
    }

    public static class Regions
    {
        public static readonly Guid Latam = Guid.Parse("218c74bc-b109-4284-908e-6cdf4983e543");
        public static readonly Guid NorthAmerica = Guid.Parse("7131dd68-26cf-44a5-a75e-5038167b8720");
    }

    public static class Countries
    {
        public static readonly Guid Mexico = Guid.Parse("fb991387-3247-4463-886b-f97154cea0b4");
        public static readonly Guid Colombia = Guid.Parse("6e48c72b-3ae8-4646-bd86-3f46d39fd8b3");
        public static readonly Guid CostaRica = Guid.Parse("1f8d06f3-def6-40a0-b8bd-b3a44c03f1c2");
        public static readonly Guid UnitedStates = Guid.Parse("a82cdef6-b8ac-4c76-81c8-9cdd8410485a");
    }

    public static class Accounts
    {
        public static readonly Guid NovaBank = Guid.Parse("0acd2297-f9b6-46c0-a26d-18868180a1eb");
        public static readonly Guid Streamly = Guid.Parse("00fffde5-996f-4181-8b55-045c220416c9");
        public static readonly Guid Shopora = Guid.Parse("797da587-e74c-4f1e-b0d8-841eb056d524");
        public static readonly Guid AeroLink = Guid.Parse("54bed65c-cecd-4c0d-a8b0-ad4483912535");
    }

    public static class Campaigns
    {
        public static readonly Guid NovaBankCreditCard = Guid.Parse("7942833e-9511-421c-8a19-a51dff9330e7");
        public static readonly Guid NovaBankFraud = Guid.Parse("bd48e8c7-8acd-465f-8e30-8921fe04140d");
        public static readonly Guid StreamlyCreator = Guid.Parse("5c93320c-3e90-4a17-9ac7-57e7d701aa67");
        public static readonly Guid ShoporaAccess = Guid.Parse("5895a383-db7f-4ca7-b17e-62fa6a5bda0d");
        public static readonly Guid AeroLinkTravel = Guid.Parse("ed4e4d95-45f4-47d4-811a-68c595cfe921");
    }

    public static class Customers
    {
        public static readonly Guid NovaBankCustomer1 = Guid.Parse("a1b2c3d4-e5f6-4711-8001-000000000001");
        public static readonly Guid StreamlyCustomer1 = Guid.Parse("a1b2c3d4-e5f6-4711-8002-000000000002");
        public static readonly Guid ShoporaCustomer1 = Guid.Parse("a1b2c3d4-e5f6-4711-8003-000000000003");
        public static readonly Guid AeroLinkCustomer1 = Guid.Parse("a1b2c3d4-e5f6-4711-8004-000000000004");
    }

    public static class SlaPolicies
    {
        public static readonly Guid CriticalPriorityDefault = Guid.Parse("d8d5f8b2-5d7f-4ca5-bf09-0e060a8afc01");
        public static readonly Guid HighPriorityDefault = Guid.Parse("aaf223ad-9e58-4055-9513-178e29085210");
        public static readonly Guid NormalPriorityDefault = Guid.Parse("38394626-fad4-4819-9bb6-72eb353fe7e6");
        public static readonly Guid LowPriorityDefault = Guid.Parse("f58a9769-4706-4728-ad78-0d931d2d472a");
    }

    public static class UserScopes
    {
        public static readonly Guid ManagerLatam = Guid.Parse("c616e064-8180-4aef-aa8b-cae17d68ef1d");
        public static readonly Guid SupervisorNovabank = Guid.Parse("8d09bd50-386d-496f-9bdb-ddaea22fd0ac");
        public static readonly Guid AgentNovabankCreditCard = Guid.Parse("156aafc5-16ac-4883-8d63-c944bce1bda7");
        public static readonly Guid ViewerLatam = Guid.Parse("c1d256d9-9595-4dc4-84e0-234085d208a5");
    }

    public static class Tickets
    {
        public static readonly Guid NovaBankOpen = Guid.Parse("314cdb81-8d1a-41c1-9a00-000000000001");
        public static readonly Guid NovaBankAssigned = Guid.Parse("314cdb81-8d1a-41c1-9a00-000000000002");
        public static readonly Guid NovaBankInProgress = Guid.Parse("314cdb81-8d1a-41c1-9a00-000000000003");
        public static readonly Guid NovaBankEscalated = Guid.Parse("314cdb81-8d1a-41c1-9a00-000000000004");
        public static readonly Guid NovaBankResolved = Guid.Parse("314cdb81-8d1a-41c1-9a00-000000000005");
        public static readonly Guid NovaBankClosed = Guid.Parse("314cdb81-8d1a-41c1-9a00-000000000006");
    }

    public static class TicketSlaStates
    {
        public static readonly Guid NovaBankOpen = Guid.Parse("322cdb81-8d1a-41c1-9a00-000000000001");
        public static readonly Guid NovaBankAssigned = Guid.Parse("322cdb81-8d1a-41c1-9a00-000000000002");
        public static readonly Guid NovaBankInProgress = Guid.Parse("322cdb81-8d1a-41c1-9a00-000000000003");
        public static readonly Guid NovaBankEscalated = Guid.Parse("322cdb81-8d1a-41c1-9a00-000000000004");
        public static readonly Guid NovaBankResolved = Guid.Parse("322cdb81-8d1a-41c1-9a00-000000000005");
        public static readonly Guid NovaBankClosed = Guid.Parse("322cdb81-8d1a-41c1-9a00-000000000006");
    }

    public static class TicketAssignments
    {
        public static readonly Guid NovaBankAssigned = Guid.Parse("333cdb81-8d1a-41c1-9a00-000000000002");
        public static readonly Guid NovaBankInProgress = Guid.Parse("333cdb81-8d1a-41c1-9a00-000000000003");
        public static readonly Guid NovaBankEscalated = Guid.Parse("333cdb81-8d1a-41c1-9a00-000000000004");
        public static readonly Guid NovaBankResolved = Guid.Parse("333cdb81-8d1a-41c1-9a00-000000000005");
        public static readonly Guid NovaBankClosed = Guid.Parse("333cdb81-8d1a-41c1-9a00-000000000006");
    }

    public static class TicketStatusHistory
    {
        public static readonly Guid NovaBankOpen = Guid.Parse("344cdb81-8d1a-41c1-9a00-000000000001");
        public static readonly Guid NovaBankAssigned = Guid.Parse("344cdb81-8d1a-41c1-9a00-000000000002");
        public static readonly Guid NovaBankInProgress = Guid.Parse("344cdb81-8d1a-41c1-9a00-000000000003");
        public static readonly Guid NovaBankEscalated = Guid.Parse("344cdb81-8d1a-41c1-9a00-000000000004");
        public static readonly Guid NovaBankResolved = Guid.Parse("344cdb81-8d1a-41c1-9a00-000000000005");
        public static readonly Guid NovaBankClosed = Guid.Parse("344cdb81-8d1a-41c1-9a00-000000000006");
    }

    public static class TicketComments
    {
        public static readonly Guid NovaBankInProgress = Guid.Parse("355cdb81-8d1a-41c1-9a00-000000000003");
        public static readonly Guid NovaBankEscalated = Guid.Parse("355cdb81-8d1a-41c1-9a00-000000000004");
    }

    public static class TicketEscalations
    {
        public static readonly Guid NovaBankEscalated = Guid.Parse("366cdb81-8d1a-41c1-9a00-000000000004");
    }

    public static class TicketResolutions
    {
        public static readonly Guid NovaBankResolved = Guid.Parse("377cdb81-8d1a-41c1-9a00-000000000005");
        public static readonly Guid NovaBankClosed = Guid.Parse("377cdb81-8d1a-41c1-9a00-000000000006");
    }
}
