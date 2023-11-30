// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

library LeaderboardLib {
    struct Player {
        address user;
        int score;
    }

    function pop(Player[10] storage array) internal {
        for (uint256 i = 9; i > 0; i--) {
            array[i] = array[i - 1];
        }
    }
}

contract MathGameSM {
    using LeaderboardLib for LeaderboardLib.Player[10];

    LeaderboardLib.Player[10] public topPlayers;
    mapping(address => int) public userScores;

    uint256 public deploymentTime;
    uint256 public constant threeDaysInSeconds = 3 * 24 * 60 * 60; // 3 days in seconds
    uint256 public lastPrizeDistribution;
    bool public prizeDistributed;

    event ScoreUpdated(address indexed user, int newScore);
    event PrizeDistributed(address[] recipients, int[] rewards);

    modifier onlyOnce {
        require(!prizeDistributed, "Prize already distributed");
        _;
    }

    modifier threeDaysPassed {
        require(block.timestamp >= lastPrizeDistribution + threeDaysInSeconds, "Three days have not passed");
        _;
    }

    constructor() {
        deploymentTime = block.timestamp;
        lastPrizeDistribution = deploymentTime;
    }

    function updateScore(int scoreDelta) external {
        address user = msg.sender;

        // Update user score
        userScores[user] += scoreDelta;

        // Emit event
        emit ScoreUpdated(user, userScores[user]);

        // Update leaderboard
        updateLeaderboard(user);
    }

    function getTopPlayers() external view returns (LeaderboardLib.Player[10] memory) {
        return topPlayers;
    }

    function updateLeaderboard(address user) internal {
        // Find the position to insert the user in the leaderboard
        uint256 insertIndex = 0;
        while (insertIndex < topPlayers.length && userScores[user] < topPlayers[insertIndex].score) {
            insertIndex++;
        }

        // Shift the array to make space for the new user
        topPlayers.pop();

        for (uint256 i = topPlayers.length - 1; i > insertIndex; i--) {
            topPlayers[i] = topPlayers[i - 1];
        }

        // Insert the user into the leaderboard
        topPlayers[insertIndex] = LeaderboardLib.Player(user, userScores[user]);
    }

    function distributePrize() external onlyOnce threeDaysPassed {
        address[] memory recipients = new address[](topPlayers.length);
        int[] memory rewards = new int[](topPlayers.length);

        for (uint256 i = 0; i < topPlayers.length; i++) {
            recipients[i] = topPlayers[i].user;
            rewards[i] = calculateReward(i + 1); // Adjust the reward calculation as needed
        }

        // Distribute prizes
        // (Assuming you have a token contract with a transfer function)
        // tokenContract.transfer(recipients[i], rewards[i]);

        // Emit event
        emit PrizeDistributed(recipients, rewards);

        // Mark prize as distributed
        prizeDistributed = true;

        // Update last prize distribution time
        lastPrizeDistribution = block.timestamp;
    }

    function calculateReward(uint256 rank) internal pure returns (int) {
        // Placeholder function for reward calculation, adjust as needed
        return int(10 * rank); // Example: 10 tokens for rank 1, 20 tokens for rank 2, and so on.
    }

    function timeUntilNextPrizeDistribution() external view returns (uint256) {
        if (block.timestamp >= lastPrizeDistribution + threeDaysInSeconds) {
            return 0;
        } else {
            return lastPrizeDistribution + threeDaysInSeconds - block.timestamp;
        }
    }
}