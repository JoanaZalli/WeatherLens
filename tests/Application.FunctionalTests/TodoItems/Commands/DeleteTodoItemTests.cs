using WeatherLens.Application.TodoItems.Commands.CreateTodoItem;
using WeatherLens.Application.TodoItems.Commands.DeleteTodoItem;
using WeatherLens.Application.TodoLists.Commands.CreateTodoList;
using WeatherLens.Domain.Entities;

using static WeatherLens.Application.FunctionalTests.Testing;

namespace WeatherLens.Application.FunctionalTests.TodoItems.Commands;
public class DeleteTodoItemTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoItemId()
    {
        var command = new DeleteTodoItemCommand(99);

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeleteTodoItem()
    {
        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var itemId = await SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "New Item"
        });

        await SendAsync(new DeleteTodoItemCommand(itemId));

        var item = await FindAsync<TodoItem>(itemId);

        item.Should().BeNull();
    }
}
