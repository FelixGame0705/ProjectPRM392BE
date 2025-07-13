// MappingProfile.cs (Updated)
using AutoMapper;
using GoEStores.Core.DTO.Responses;
using GoEStores.Repositories.Entity;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));
            CreateMap<CreateProductDto, Product>();

            // Category mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Cart mappings
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null));
            CreateMap<CreateCartDto, Cart>();
            CreateMap<UpdateCartDto, Cart>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // CartItem mappings
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : null))
                .ForMember(dest => dest.ProductImageURL, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageURL : null));

            CreateMap<CreateCartItemDto, CartItem>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()) // Price will be set from Product
                .ForMember(dest => dest.CartID, opt => opt.Ignore()); // CartID will be set in service

            CreateMap<UpdateCartItemDto, CartItem>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()) // Price will be updated from Product if ProductID changes
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()); // Will be calculated manually in service
            CreateMap<CreateOrderDto, Order>();

            // Payment mappings
            CreateMap<Payment, PaymentDto>();
            CreateMap<CreatePaymentDto, Payment>();
            CreateMap<NotificationDto, Notification>();
            CreateMap<Notification, NotificationDto>();
            CreateMap<CreateNotificationDto, Notification>().ReverseMap();


            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString())); // Assuming Role is an enum in User entity
            CreateMap<User, LoginDto>().ReverseMap()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Password will be hashed in service

            CreateMap<ResponseChatMessage, ChatMessage>().ReverseMap();
            CreateMap<ChatHub, ChatHubResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
            .ForMember(dest => dest.UpdatedTime, opt => opt.MapFrom(src => src.UpdatedTime))
            .ForMember(dest => dest.FUserId, opt => opt.MapFrom(src => src.FUserId ?? 0)) // Xử lý nullable
            .ForMember(dest => dest.SUserId, opt => opt.MapFrom(src => src.SUserId))
            .ForMember(dest => dest.ChatMessages, opt => opt.MapFrom(src => src.ChatMessages));

            CreateMap<Notification, NotificationResponse>();

            // StoreLocation mappings
            CreateMap<StoreLocation, StoreLocationDto>()
                .ForMember(dest => dest.DistanceKm, opt => opt.Ignore()); // Will be calculated in service
            CreateMap<CreateStoreLocationDto, StoreLocation>();
            CreateMap<UpdateStoreLocationDto, StoreLocation>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}