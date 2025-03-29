using Microsoft.AspNetCore.Mvc;
using RecruitmentTask.Api.Mappers;
using RecruitmentTask.Contracts.Wallets.Requests;
using RecruitmentTask.Contracts.Wallets.Responses;
using RecruitmentTask.Core.ExchangeRate;
using RecruitmentTask.Core.Validations;
using RecruitmentTask.Core.Wallets;
using ValidationError = RecruitmentTask.Contracts.Validations.ValidationError;
using ValidationResult = RecruitmentTask.Contracts.Validations.ValidationResult;

namespace RecruitmentTask.Api.Wallet
{
    [Route("/wallets")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletDomainService _walletDomainService;
        private readonly IExchangeRatesRepository _exchangeRatesRepository;
        
        public WalletsController(
            IWalletDomainService walletDomainService,
            IExchangeRatesRepository exchangeRatesRepository)
        {
            _walletDomainService = walletDomainService;
            _exchangeRatesRepository = exchangeRatesRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetWallets), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromQuery] int pageNumber, [FromQuery] short countPerPage)
        {
            var validation = Validate(pageNumber, countPerPage);

            if (validation.Errors.Any())
            {
                return BadRequest(validation);
            }

            var wallets = await _walletDomainService.GetWallets(pageNumber, countPerPage);

            var rates = await _exchangeRatesRepository.GetMostRecentExchangeRates();

            return Ok(wallets.ToContract(rates));
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateWalletRequest createWalletRequest)
        {
            var validation = Validate(createWalletRequest);

            if (validation.Errors.Any())
            {
                return BadRequest(validation);
            }

            var operationResult = await _walletDomainService.Create(createWalletRequest.Name);

            if (operationResult.IsSuccess)
            {
                return Ok(new CreationResponse{Id = operationResult.Value});
            }

            return BadRequest(operationResult.ToContracts());
        }

        [HttpPut("{id}/deposit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deposit([FromRoute] int id,
            [FromBody] DepositOrWithdrawalRequest depositRequest)
        {
            var operationResult =
                await _walletDomainService.Deposit(id, depositRequest.CurrencyCode, depositRequest.Amount);

            if (operationResult.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(operationResult.ToContracts());
        }

        [HttpPut("{id}/withdrawal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Withdrawal([FromRoute] int id,
            [FromBody] DepositOrWithdrawalRequest depositRequest)
        {
            var operationResult =
                await _walletDomainService.Withdrawal(id, depositRequest.CurrencyCode, depositRequest.Amount);

            if (operationResult.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(operationResult.ToContracts());
        }

        [HttpPut("{id}/convert")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Convert([FromRoute] int id, [FromBody] ConvertRequest convertRequest)
        {
            var operationResult = await _walletDomainService.Convert(id, convertRequest.FromCurrencyCode,
                convertRequest.ToCurrencyCode, convertRequest.Amount);

            if (operationResult.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(operationResult.ToContracts());
        }

        private ValidationResult Validate(CreateWalletRequest request)
        {
            var errors = new List<ValidationError>();

            if (request.Name.Length > 150)
            {
                errors.Add(new ValidationError
                    { ErrorCode = ErrorCodes.ValueTooLong, PropertyName = nameof(request.Name) });
            }

            return new ValidationResult { Errors = errors };
        }

        private ValidationResult Validate(int pageNumber, short countPerPage)
        {
            var errors = new List<ValidationError>();

            if (pageNumber < 1)
            {
                errors.Add(new ValidationError
                    { ErrorCode = ErrorCodes.ValueCannotBeLessOrEqualZero, PropertyName = nameof(pageNumber) });
            }

            if (countPerPage < 1)
            {
                errors.Add(new ValidationError
                    { ErrorCode = ErrorCodes.ValueCannotBeLessOrEqualZero, PropertyName = nameof(countPerPage) });
            }

            if (countPerPage > 100)
            {
                errors.Add(new ValidationError
                    { ErrorCode = ErrorCodes.ValueTooLong, PropertyName = nameof(countPerPage) });
            }

            return new ValidationResult { Errors = errors };
        }
    }
}
