using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AuctionService.UnitTests
{
    public class AuctionControllerTests
    {
        private readonly Mock<IAuctionRepository> _auctionRepo;

        private readonly Mock<IPublishEndpoint> _publishEndpoint;

        private readonly Fixture _fixture;

        private readonly AuctionController _controller;

        private readonly IMapper _mapper;

        public AuctionControllerTests()
        {
            _fixture = new Fixture();
            _auctionRepo = new Mock<IAuctionRepository>();
            _publishEndpoint = new Mock<IPublishEndpoint>();

            var mockLoggerFactory = new NullLoggerFactory();
            var mockMapper = new MapperConfiguration(mc =>
            {
                mc.AddMaps(typeof(MappingProfiles).Assembly);

            }, mockLoggerFactory).CreateMapper().ConfigurationProvider;

            _mapper = new Mapper(mockMapper);
            _controller = new AuctionController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = Helpers.GetClaimsPrincipal()
                    }
                }
            };
        }

        [Fact]
        public async Task GetAllAuctions_WithNoParams_Returns10Auctions()
        {
            // arrange
            var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
            _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

            // act
            var result = await _controller.GetAllAuctions(null);

            // assert
            Assert.Equal(10, result.Value.Count);
            Assert.IsType<ActionResult<List<AuctionDto>>>(result);
        }

        [Fact]
        public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
        {
            // arrange
            var auction = _fixture.Create<AuctionDto>();
            _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

            // act
            var result = await _controller.GetAuctionById(auction.Id);

            // assert
            Assert.Equal(auction.Make, result.Value.Make);
            Assert.IsType<ActionResult<AuctionDto>>(result);
        }

        [Fact]
        public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
        {
            // arrange
            _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

            // act
            var result = await _controller.GetAuctionById(Guid.NewGuid());

            // assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
        {
            // arrange
            var auction = _fixture.Create<CreateAuctionDto>();
            //_auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // act
            var result = await _controller.CreateAuction(auction);
            var createdResult = result.Result as CreatedAtActionResult;

            // assert
            Assert.NotNull(createdResult);
            Assert.Equal("GetAuctionById", createdResult.ActionName);
            Assert.IsType<AuctionDto>(createdResult.Value);
        }

        [Fact]
        public async Task CreateAuction_FailedSave_Returns400BadRequest()
        {
            // arrange
            var auction = _fixture.Create<CreateAuctionDto>();
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

            // act
            var result = await _controller.CreateAuction(auction);

            // assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
            auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
            auction.Seller = Helpers.GetClaimsPrincipal().Identity.Name;
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

            var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
            auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

            var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateAuction_FailedSave_ReturnsBadRequest()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
            auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
            auction.Seller = Helpers.GetClaimsPrincipal().Identity.Name;
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

            var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            auction.Seller = Helpers.GetClaimsPrincipal().Identity.Name;
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _controller.DeleteAuction(auction.Id);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

            var result = await _controller.DeleteAuction(auction.Id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithInvalidUser_Returns403Response()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

            var result = await _controller.DeleteAuction(auction.Id);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_FailedSave_ReturnsBadRequest()
        {
            var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
            auction.Seller = Helpers.GetClaimsPrincipal().Identity.Name;
            _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
            _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

            var result = await _controller.DeleteAuction(auction.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
