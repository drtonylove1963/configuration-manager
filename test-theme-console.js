// Console test for theme system
// Copy and paste this into the browser console to test the theme system

console.log('=== THEME SYSTEM TEST ===');

// Test 1: Check if functions exist
console.log('1. Checking if theme functions exist...');
console.log('window.currentTheme:', typeof window.currentTheme, window.currentTheme);
console.log('window.setTheme:', typeof window.setTheme);
console.log('window.toggleTheme:', typeof window.toggleTheme);

// Test 2: Test setTheme function
console.log('\n2. Testing setTheme function...');
try {
    console.log('Setting theme to dark...');
    window.setTheme('dark');
    console.log('Current theme after setting dark:', window.currentTheme);
    console.log('Body classes:', document.body.className);
    
    setTimeout(() => {
        console.log('Setting theme to light...');
        window.setTheme('light');
        console.log('Current theme after setting light:', window.currentTheme);
        console.log('Body classes:', document.body.className);
    }, 2000);
} catch (error) {
    console.error('Error in setTheme test:', error);
}

// Test 3: Test toggleTheme function
setTimeout(() => {
    console.log('\n3. Testing toggleTheme function...');
    try {
        console.log('Current theme before toggle:', window.currentTheme);
        const result = window.toggleTheme();
        console.log('Toggle result:', result);
        console.log('Current theme after toggle:', window.currentTheme);
        console.log('Body classes:', document.body.className);
    } catch (error) {
        console.error('Error in toggleTheme test:', error);
    }
}, 4000);

// Test 4: Check localStorage
setTimeout(() => {
    console.log('\n4. Testing localStorage...');
    try {
        const saved = localStorage.getItem('theme');
        console.log('Theme in localStorage:', saved);
    } catch (error) {
        console.error('Error checking localStorage:', error);
    }
}, 6000);

// Test 5: Check Radzen components
setTimeout(() => {
    console.log('\n5. Checking Radzen components...');
    try {
        const radzenElements = document.querySelectorAll('[class*="rz-"]');
        console.log('Found Radzen elements:', radzenElements.length);
        if (radzenElements.length > 0) {
            console.log('First Radzen element classes:', radzenElements[0].className);
        }
        
        const layoutElements = document.querySelectorAll('.rz-layout, .rz-body, .rz-content, .rz-sidebar, .rz-header');
        console.log('Found layout elements:', layoutElements.length);
        if (layoutElements.length > 0) {
            console.log('First layout element classes:', layoutElements[0].className);
        }
    } catch (error) {
        console.error('Error checking Radzen components:', error);
    }
}, 8000);

console.log('\n=== TEST COMPLETE - Check results above ===');
