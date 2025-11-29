using Moq;
using Relevo.Core.Interfaces;
using Relevo.UseCases.ShiftCheckIn;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relevo.Core.Services;

namespace Relevo.UnitTests.UseCases.ShiftCheckIn;

public class ShiftCheckInServiceTests
{
    private readonly Mock<IUnitRepository> _mockUnitRepository;
    private readonly Mock<IShiftRepository> _mockShiftRepository;
    private readonly Mock<IPatientRepository> _mockPatientRepository;
    private readonly Mock<IAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IHandoverRepository> _mockHandoverRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IShiftBoundaryResolver> _mockShiftBoundaryResolver;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly GetUnitsUseCase _getUnitsUseCase;
    private readonly GetShiftsUseCase _getShiftsUseCase;
    private readonly GetPatientsByUnitUseCase _getPatientsByUnitUseCase;
    private readonly GetAllPatientsUseCase _getAllPatientsUseCase;
    private readonly GetMyPatientsUseCase _getMyPatientsUseCase;
    private readonly GetMyHandoversUseCase _getMyHandoversUseCase;
    private readonly GetPatientHandoversUseCase _getPatientHandoversUseCase;
    private readonly GetHandoverByIdUseCase _getHandoverByIdUseCase;
    private readonly GetPatientByIdUseCase _getPatientByIdUseCase;
    private readonly AssignPatientsUseCase _assignPatientsUseCase;
    private readonly ShiftCheckInService _service;

    public ShiftCheckInServiceTests()
    {
        _mockUnitRepository = new Mock<IUnitRepository>();
        _mockShiftRepository = new Mock<IShiftRepository>();
        _mockPatientRepository = new Mock<IPatientRepository>();
        _mockAssignmentRepository = new Mock<IAssignmentRepository>();
        _mockHandoverRepository = new Mock<IHandoverRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockShiftBoundaryResolver = new Mock<IShiftBoundaryResolver>();
        _mockUserContext = new Mock<IUserContext>();

        _getUnitsUseCase = new GetUnitsUseCase(_mockUnitRepository.Object);
        _getShiftsUseCase = new GetShiftsUseCase(_mockShiftRepository.Object);
        _getPatientsByUnitUseCase = new GetPatientsByUnitUseCase(_mockPatientRepository.Object);
        _getAllPatientsUseCase = new GetAllPatientsUseCase(_mockPatientRepository.Object);
        _getMyPatientsUseCase = new GetMyPatientsUseCase(_mockAssignmentRepository.Object);
        _getMyHandoversUseCase = new GetMyHandoversUseCase(_mockHandoverRepository.Object);
        _getPatientHandoversUseCase = new GetPatientHandoversUseCase(_mockHandoverRepository.Object);
        _getHandoverByIdUseCase = new GetHandoverByIdUseCase(_mockHandoverRepository.Object);
        _getPatientByIdUseCase = new GetPatientByIdUseCase(_mockPatientRepository.Object);
        _assignPatientsUseCase = new AssignPatientsUseCase(_mockAssignmentRepository.Object, _mockUserRepository.Object, _mockShiftBoundaryResolver.Object, _mockUserContext.Object);

        // For other repos, create mocks if needed for ShiftCheckInService
        var mockHandoverParticipantsRepository = new Mock<IHandoverParticipantsRepository>();
        var mockHandoverSyncStatusRepository = new Mock<IHandoverSyncStatusRepository>();
        var mockHandoverMessagingRepository = new Mock<IHandoverMessagingRepository>();
        var mockHandoverActivityRepository = new Mock<IHandoverActivityRepository>();
        var mockHandoverChecklistRepository = new Mock<IHandoverChecklistRepository>();
        var mockHandoverContingencyRepository = new Mock<IHandoverContingencyRepository>();
        var mockHandoverActionItemsRepository = new Mock<IHandoverActionItemsRepository>();
        var mockPatientSummaryRepository = new Mock<IPatientSummaryRepository>();
        var mockHandoverSectionsRepository = new Mock<IHandoverSectionsRepository>();

        _service = new ShiftCheckInService(
            _assignPatientsUseCase,
            _getMyPatientsUseCase,
            _getMyHandoversUseCase,
            _getUnitsUseCase,
            _getShiftsUseCase,
            _getPatientsByUnitUseCase,
            _getAllPatientsUseCase,
            _getPatientHandoversUseCase,
            _getHandoverByIdUseCase,
            _getPatientByIdUseCase,
            mockHandoverParticipantsRepository.Object,
            mockHandoverSyncStatusRepository.Object,
            _mockUserRepository.Object,
            mockHandoverMessagingRepository.Object,
            mockHandoverActivityRepository.Object,
            mockHandoverChecklistRepository.Object,
            mockHandoverContingencyRepository.Object,
            mockHandoverActionItemsRepository.Object,
            mockPatientSummaryRepository.Object,
            _mockHandoverRepository.Object,
            _mockAssignmentRepository.Object,
            mockHandoverSectionsRepository.Object);
    }

    [Fact]
    public async Task GetUnitsAsync_ShouldReturnUnits()
    {
        // Arrange
        var expectedUnits = new List<UnitRecord> { new UnitRecord("1", "Unit1") };
        _mockUnitRepository.Setup(r => r.GetUnits()).Returns(expectedUnits);

        // Act
        var result = await _service.GetUnitsAsync();

        // Assert
        Assert.Equal(expectedUnits, result);
        _mockUnitRepository.Verify(r => r.GetUnits(), Times.Once);
    }

    [Fact]
    public async Task GetShiftsAsync_ShouldReturnShifts()
    {
        // Arrange
        var expectedShifts = new List<ShiftRecord> { new ShiftRecord("1", "Shift1", "08:00", "16:00") };
        _mockShiftRepository.Setup(r => r.GetShifts()).Returns(expectedShifts);

        // Act
        var result = await _service.GetShiftsAsync();

        // Assert
        Assert.Equal(expectedShifts, result);
        _mockShiftRepository.Verify(r => r.GetShifts(), Times.Once);
    }

    [Fact]
    public async Task GetMyPatientsAsync_ShouldReturnPatients()
    {
        // Arrange
        var userId = "user1";
        var page = 1;
        var pageSize = 10;
        var expected = (new List<PatientRecord>(), 5);
        _mockAssignmentRepository.Setup(r => r.GetMyPatients(userId, page, pageSize)).Returns(expected);

        // Act
        var result = await _service.GetMyPatientsAsync(userId, page, pageSize);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task AssignPatientsAsync_ShouldCallUseCase()
    {
        // Arrange
        var userId = "user1";
        var shiftId = "shift1";
        var patientIds = new[] { "patient1" };
        _mockAssignmentRepository.Setup(r => r.AssignAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<string> { "assign1" });
        _mockUserRepository.Setup(r => r.EnsureUserExists(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()));

        // Act
        await _service.AssignPatientsAsync(userId, shiftId, patientIds);

        // Assert
        _mockAssignmentRepository.Verify(r => r.AssignAsync(userId, shiftId, patientIds), Times.Once);
        _mockUserRepository.Verify(r => r.EnsureUserExists(userId, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
    }
}
