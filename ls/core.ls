;; Binding test
(define box (fn (contents title) (. 'System.Windows.Forms.MessageBox 'Show contents title (. 'System.Windows.Forms.MessageBoxButtons 'OKCancel) (. 'System.Windows.Forms.MessageBoxIcon 'Error))))

;; Helpers
(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))
(define read (fn (str) (. 'ls.Reader 'Read str)))
(define eval (fn (str) (. 'ls.Evaluator 'Eval str *env*)))

;; Math
(define pi (. 'System.Math 'PI))
(define tau (* pi 2))
(define sin (fn (x) (. 'System.Math 'Sin x)))
(define cos (fn (x) (. 'System.Math 'Cos x)))
(define tan (fn (x) (. 'System.Math 'Tan x)))
(define rad (fn (x) (* pi (/ x 180.0))))
(define deg (fn (x) (* x (/ 180.0 pi))))

;; Looping
;; example: (dotimes 3 '(print "yo"))
(define dotimes (fn (i expr)
  (if (= i 0)
    'done
    (begin
      (eval expr)
      (dotimes (- i 1) expr)))))
