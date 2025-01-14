// test.js
import { check } from 'k6';
import http from 'k6/http';
import { sleep } from 'k6';


const TOTAL_USERS = 100;

export const options = {
    scenarios: {
        constant_request_rate: {
            executor: 'constant-arrival-rate',
            rate: 1000, // 1000 iterations per second
            timeUnit: '1s',
            duration: '100s',
            preAllocatedVUs: TOTAL_USERS / 2, // Initial pool of VUs
            maxVUs: TOTAL_USERS, // Maximum pool of VUs
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
        http_req_failed: ['rate<0.01'], // Less than 1% of requests should fail
    },
};

// Constants

const BASE_URL = 'http://localhost:5000'; // Replace with your actual API endpoint

// Utility functions
function generateUniqueId(prefix = '') {
    return `${prefix}${Math.random().toString(36).substring(2)}${Date.now()}`;
}

function getRandomUserId(totalUsers) {
    return `user_${Math.floor(Math.random() * totalUsers)}`;
}

function generatePayload(userId) {
    const activityId = generateUniqueId('act_');
    const dataId = generateUniqueId('data_');
    
    return {
        userId,
        activity: {
            id: activityId,
            name: `Activity`,
            type: 'news',
            data: {
                id: dataId,
                title: `News Title`
            }
        }
    };
}

// Main test function
export default function () {
    const userId = getRandomUserId(TOTAL_USERS);
    const payload = generatePayload(userId);

    const headers = {
        'Content-Type': 'application/json',
        // Add any other required headers here
    };

    const response = http.post(
        `${BASE_URL}/activity`,
        JSON.stringify(payload),
        { headers }
    );

    check(response, {
        'is status 200': (r) => r.status === 200,
        'transaction time < 500ms': (r) => r.timings.duration < 500,
    });

    sleep(0.1); // Add small sleep to prevent overwhelming the system
}