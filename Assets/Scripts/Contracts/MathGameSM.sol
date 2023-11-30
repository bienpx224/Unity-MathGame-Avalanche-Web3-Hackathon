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

    event ScoreUpdated(address indexed user, int newScore);

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
}