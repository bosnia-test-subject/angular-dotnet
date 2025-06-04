using API.Interfaces;

namespace API.Data;

public class UnitOfWork(DataContext context,
IUserRepository userRepository,
ILikesRepository likesRepository,
IMessageRepository messageRepository,
ITagsRepository tagsRepository,
IPhotoRepository photoRepository) : IUnitOfWork
{
    public IUserRepository UserRepository => userRepository;
    public IMessageRepository MessageRepository => messageRepository;
    public ILikesRepository LikesRepository => likesRepository;
    public IPhotoRepository PhotosRepository => photoRepository;
    public ITagsRepository TagsRepository => tagsRepository;

    public async Task<bool> Complete()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChages()
    {
        return context.ChangeTracker.HasChanges();
    }
}
