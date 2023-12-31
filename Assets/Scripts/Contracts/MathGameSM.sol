// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

library LeaderboardLib {
    struct Player {
        address user;
        int256 score;
    }

    function pop(Player[10] storage array) internal {
        for (uint256 i = 9; i > 0; i--) {
            array[i] = array[i - 1];
        }
    }
}

contract MathGameSM {
    address public owner;
    using LeaderboardLib for LeaderboardLib.Player[10];

    LeaderboardLib.Player[10] public topPlayers;
    mapping(address => int256) public userScores;

    uint256 public deploymentTime;
    uint256 public constant threeDaysInSeconds = 3 * 24 * 60 * 60; // 3 days in seconds
    uint256 public lastPrizeDistribution;
    bool public prizeDistributed;

    address public tokenContractAddress =
        0x469940c22295554B0bEd5a1376FaeeD929bd82BF; // Thay thế bằng địa chỉ thực tế của smart contract token

    event ScoreUpdated(address indexed user, int256 newScore);
    event PrizeDistributed(address[] recipients, int256[] rewards);

    modifier onlyOnce() {
        require(!prizeDistributed, "Prize already distributed");
        _;
    }

    modifier threeDaysPassed() {
        require(
            block.timestamp >= lastPrizeDistribution + threeDaysInSeconds,
            "Three days have not passed"
        );
        _;
    }

    constructor() {
        // Gán địa chỉ của người triển khai smart contract làm chủ sở hữu
        owner = msg.sender;
        deploymentTime = block.timestamp;
        lastPrizeDistribution = deploymentTime;
    }

    function getBalance() external view returns (uint256) {
        return address(this).balance;
    }

    function testGetValue(uint256 input) external pure returns (uint256) {
        // Tăng giá trị đầu vào lên 1 và trả về
        return input + 1;
    }

    function transferTokensToAddress(address to, uint256 amount) external {
        // Check if the sender is the owner of the contract or some other authorization logic
        require(msg.sender == owner, "Not authorized to transfer tokens");

        // Check if the tokenContractAddress is set
        require(
            tokenContractAddress != address(0),
            "Token contract address not set"
        );

        // Assuming tokenContract is ERC-20 compliant with a transfer function
        // You may need to adjust this based on the actual interface of your token contract
        // Also, consider checking the return value to handle any potential errors
        (bool success, ) = address(tokenContractAddress).call(
            abi.encodeWithSelector(0xa9059cbb, to, amount)
        );
        require(success, "Token transfer failed");
    }

    function updateScore(int256 scoreDelta) external {
        address user = msg.sender;

        // Update user score
        userScores[user] += scoreDelta;

        // Emit event
        emit ScoreUpdated(user, userScores[user]);

        // Update leaderboard
        updateLeaderboard(user);
    }

    function getMyScore() external view returns (int256) {
        address user = msg.sender;
        return userScores[user];
    }

    function getTopPlayers()
        external
        view
        returns (LeaderboardLib.Player[10] memory)
    {
        return topPlayers;
    }

    // function updateLeaderboard(address user) internal {
    //     // Find the position to insert the user in the leaderboard
    //     uint256 insertIndex = 0;
    //     while (
    //         insertIndex < topPlayers.length &&
    //         userScores[user] < topPlayers[insertIndex].score
    //     ) {
    //         insertIndex++;
    //     }

    //     // Shift the array to make space for the new user
    //     topPlayers.pop();

    //     for (uint256 i = topPlayers.length - 1; i > insertIndex; i--) {
    //         topPlayers[i] = topPlayers[i - 1];
    //     }

    //     // Insert the user into the leaderboard
    //     topPlayers[insertIndex] = LeaderboardLib.Player(user, userScores[user]);
    // }

    function updateLeaderboard(address user) internal {
        // Check if the user is already in topPlayers
        bool userExists = false;
        uint256 existingIndex = 0;

        for (uint256 i = 0; i < topPlayers.length; i++) {
            if (topPlayers[i].user == user) {
                userExists = true;
                existingIndex = i;
                break;
            }
        }

        // If the user exists, update their score and reposition them in the leaderboard
        if (userExists) {
            // Save the existing user data
            LeaderboardLib.Player memory existingPlayer = topPlayers[
                existingIndex
            ];

            // Update their score
            existingPlayer.score = userScores[user];

            // Remove the existing entry from the leaderboard
            for (uint256 i = existingIndex; i < topPlayers.length - 1; i++) {
                topPlayers[i] = topPlayers[i + 1];
            }
            topPlayers.pop();

            // Find the new position to insert the user in the leaderboard
            uint256 insertIndex = 0;
            while (
                insertIndex < topPlayers.length &&
                existingPlayer.score < topPlayers[insertIndex].score
            ) {
                insertIndex++;
            }

            // Shift the array to make space for the user's new position
            topPlayers.pop();

            for (uint256 i = topPlayers.length - 1; i > insertIndex; i--) {
                topPlayers[i] = topPlayers[i - 1];
            }

            // Insert the user into the leaderboard
            topPlayers[insertIndex] = existingPlayer;
        } else {
            // Find the position to insert the user in the leaderboard
            uint256 insertIndex = 0;
            while (
                insertIndex < topPlayers.length &&
                userScores[user] < topPlayers[insertIndex].score
            ) {
                insertIndex++;
            }

            // Shift the array to make space for the new user
            topPlayers.pop();

            for (uint256 i = topPlayers.length - 1; i > insertIndex; i--) {
                topPlayers[i] = topPlayers[i - 1];
            }

            // Insert the user into the leaderboard
            topPlayers[insertIndex] = LeaderboardLib.Player(
                user,
                userScores[user]
            );
        }
    }

    function distributePrize() external onlyOnce threeDaysPassed {
        address[] memory recipients = new address[](topPlayers.length);
        int256[] memory rewards = new int256[](topPlayers.length);

        for (uint256 i = 0; i < topPlayers.length; i++) {
            recipients[i] = topPlayers[i].user;
            rewards[i] = calculateReward(i + 1); // Adjust the reward calculation as needed
        }

        // Distribute prizes
        // (Assuming you have a token contract with a transfer function)
        // Distribute prizes
        for (uint256 i = 0; i < recipients.length; i++) {
            // Assuming tokenContract is ERC-20 compliant with a transfer function
            // You may need to adjust this based on the actual interface of your token contract
            // Also, consider checking the return value to handle any potential errors
            (bool success, ) = address(tokenContractAddress).call(
                abi.encodeWithSelector(
                    0xa9059cbb,
                    recipients[i],
                    uint256(rewards[i])
                )
            );
            require(success, "Token transfer failed");
        }

        // Emit event
        emit PrizeDistributed(recipients, rewards);

        // Mark prize as distributed
        prizeDistributed = true;

        // Update last prize distribution time
        lastPrizeDistribution = block.timestamp;
    }

    function calculateReward(uint256 rank) internal pure returns (int256) {
        // Placeholder function for reward calculation, adjust as needed
        return int256(10 * rank); // Example: 10 tokens for rank 1, 20 tokens for rank 2, and so on.
    }

    function timeUntilNextPrizeDistribution() external view returns (uint256) {
        if (block.timestamp >= lastPrizeDistribution + threeDaysInSeconds) {
            return 0;
        } else {
            return lastPrizeDistribution + threeDaysInSeconds - block.timestamp;
        }
    }
}
