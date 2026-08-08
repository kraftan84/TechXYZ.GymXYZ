// The two pieces of the entry screens that need a browser: the show/hide eye and
// the strength gauge.
//
// A classic script rather than a module with JS interop, because these pages are
// rendered statically — signing in has to write a cookie, which a circuit cannot
// do, so there is no circuit here to call into. Everything below works off the
// DOM alone and degrades to a plain password field if it never runs.

(function () {
    'use strict';

    // Matches the server rule (PasswordOptions.RequiredLength). The hand-off's
    // gauge counted from 8 while its own note promised 12; 12 is the rule, so 12
    // is what the first segment rewards.
    var MIN_LENGTH = 12;

    function fieldOf(button) {
        var wrapper = button.closest('.gx-a-pw');

        return wrapper ? wrapper.querySelector('fluent-text-field, input') : null;
    }

    function bindEye(button) {
        button.addEventListener('click', function () {
            var field = fieldOf(button);
            if (!field) {
                return;
            }

            var hidden = (field.type || field.getAttribute('type')) === 'password';
            var next = hidden ? 'text' : 'password';

            // Property and attribute both: Fluent's text field mirrors the
            // attribute onto its inner input, a plain input only has the property.
            field.type = next;
            field.setAttribute('type', next);

            button.setAttribute('aria-label', hidden ? 'Masquer' : 'Afficher');
            button.setAttribute('aria-pressed', hidden ? 'true' : 'false');
        });
    }

    function score(value) {
        if (!value) {
            return 0;
        }

        var points = 0;

        if (value.length >= MIN_LENGTH) {
            points++;
        }

        if (/[a-zà-öø-ÿ]/.test(value) && /[A-ZÀ-ÖØ-Þ]/.test(value)) {
            points++;
        }

        if (/[0-9]/.test(value) || /[^A-Za-zÀ-ÿ0-9]/.test(value)) {
            points++;
        }

        return points;
    }

    function paint(gauge, value) {
        var points = score(value);
        var segments = gauge.querySelectorAll('i');

        for (var i = 0; i < segments.length; i++) {
            var rank = i + 1;
            var state = '';

            if (points >= rank) {
                state = points === rank && points < 3 ? 'mid' : 'on';
            }

            segments[i].className = state;
        }
    }

    function bindGauge(gauge) {
        var wrapper = gauge.previousElementSibling;
        var field = wrapper ? wrapper.querySelector('fluent-text-field, input') : null;

        if (!field) {
            return;
        }

        var read = function () {
            paint(gauge, field.value || '');
        };

        // "input" is what a plain field fires; Fluent's web component re-emits
        // "change" on its host, and the browser pane's scripted fill only fires
        // that one — both are wired so a typed and a scripted value look alike.
        field.addEventListener('input', read);
        field.addEventListener('change', read);
        read();
    }

    // Binding is idempotent: enhanced navigation can hand the same element back.
    function once(element, bind) {
        if (element.dataset.gxBound === '1') {
            return;
        }

        element.dataset.gxBound = '1';
        bind(element);
    }

    function start() {
        var eyes = document.querySelectorAll('.gx-a-eye');
        for (var i = 0; i < eyes.length; i++) {
            once(eyes[i], bindEye);
        }

        var gauges = document.querySelectorAll('.gx-a-strength');
        for (var j = 0; j < gauges.length; j++) {
            once(gauges[j], bindGauge);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }

    // Blazor swaps the DOM on an enhanced navigation without re-running the
    // scripts on the page. Without this, following "Mot de passe oublié ?" gives
    // a screen whose eye does nothing.
    document.addEventListener('enhancedload', start);
})();
