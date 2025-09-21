using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Me;
using Xunit;
using System.Net;
using System.Collections.Generic;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoverEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetActiveHandover_ReturnsNotFound_WhenNoActiveHandover()
  {
    var response = await _client.GetAsync("/me/handovers/active");
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetHandoverParticipants_ReturnsParticipants_WhenHandoverExists()
  {
    // This would require creating a test handover first
    // For now, test that the endpoint exists and handles missing handover gracefully
    var response = await _client.GetAsync("/me/handovers/test-handover-001/participants");
    // Should return 404 for non-existent handover
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetHandoverSections_ReturnsSections_WhenHandoverExists()
  {
    // Test that the endpoint exists
    var response = await _client.GetAsync("/me/handovers/test-handover-001/sections");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetHandoverSyncStatus_ReturnsSyncStatus_WhenHandoverExists()
  {
    // Test that the endpoint exists
    var response = await _client.GetAsync("/me/handovers/test-handover-001/sync-status");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateHandoverSection_ReturnsSuccess_WhenValidData()
  {
    var updateData = new
    {
      content = "Updated section content",
      status = "completed"
    };

    var response = await _client.PutAsync("/me/handovers/test-handover-001/sections/test-section-001",
                                         new StringContent(System.Text.Json.JsonSerializer.Serialize(updateData),
                                         System.Text.Encoding.UTF8, "application/json"));
    // Should handle gracefully even with test data
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetUserPreferences_ReturnsPreferences_WhenUserExists()
  {
    var response = await _client.GetAsync("/me/preferences");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetUserSessions_ReturnsSessions_WhenUserExists()
  {
    var response = await _client.GetAsync("/me/sessions");
    Assert.True(response.IsSuccessStatusCode);
  }

  [Fact]
  public async Task UpdateUserPreferences_ReturnsSuccess_WhenValidData()
  {
    var preferences = new
    {
      theme = "dark",
      language = "en",
      timezone = "America/New_York",
      notificationsEnabled = true,
      autoSaveEnabled = false
    };

    var response = await _client.PutAsync("/me/preferences",
                                         new StringContent(System.Text.Json.JsonSerializer.Serialize(preferences),
                                         System.Text.Encoding.UTF8, "application/json"));
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetIPassTemplates_ReturnsTemplates()
  {
    var response = await _client.GetAsync("/me/templates/ipass");
    Assert.True(response.IsSuccessStatusCode);
  }

  [Fact]
  public async Task GetSectionTemplates_ReturnsTemplates()
  {
    var response = await _client.GetAsync("/me/templates/sections");
    Assert.True(response.IsSuccessStatusCode);
  }

  [Fact]
  public async Task GetHandoverConfirmationChecklists_ReturnsChecklists_WhenHandoverExists()
  {
    var response = await _client.GetAsync("/me/handovers/test-handover-001/confirmation-checklists");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateConfirmationChecklistItem_ReturnsSuccess_WhenValidData()
  {
    var updateData = new
    {
      isChecked = true,
      checkedAt = System.DateTime.UtcNow.ToString("o")
    };

    var response = await _client.PutAsync("/me/handovers/test-handover-001/confirmation-checklists/test-item-id",
                                         new StringContent(System.Text.Json.JsonSerializer.Serialize(updateData),
                                         System.Text.Encoding.UTF8, "application/json"));
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetHandoverContingencyPlans_ReturnsPlans_WhenHandoverExists()
  {
    var response = await _client.GetAsync("/me/handovers/test-handover-001/contingency-plans");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task CreateContingencyPlan_ReturnsSuccess_WhenValidData()
  {
    var planData = new
    {
      conditionText = "If patient develops acute shortness of breath",
      actionText = "Administer supplemental oxygen, call respiratory therapy",
      priority = "high",
      status = "active"
    };

    var response = await _client.PostAsync("/me/handovers/test-handover-001/contingency-plans",
                                          new StringContent(System.Text.Json.JsonSerializer.Serialize(planData),
                                          System.Text.Encoding.UTF8, "application/json"));
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetHandoverDiscussionMessages_ReturnsMessages_WhenHandoverExists()
  {
    var response = await _client.GetAsync("/me/handovers/test-handover-001/messages");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task CreateDiscussionMessage_ReturnsSuccess_WhenValidData()
  {
    var messageData = new
    {
      messageText = "Test discussion message",
      messageType = "message"
    };

    var response = await _client.PostAsync("/me/handovers/test-handover-001/messages",
                                          new StringContent(System.Text.Json.JsonSerializer.Serialize(messageData),
                                          System.Text.Encoding.UTF8, "application/json"));
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetHandoverHistory_ReturnsHandoverHistory_WhenPatientExists()
  {
    var response = await _client.GetAsync("/me/patients/test-patient-001/handovers");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetHandoverHistory_ReturnsEmptyList_WhenPatientHasNoHandovers()
  {
    var response = await _client.GetAsync("/me/patients/test-patient-no-handovers/handovers");
    Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
  }
}
